<#
.SYNOPSIS
    One-time setup of the local Kubernetes cluster (Docker Desktop) for GitOps deployment with Argo CD.

.DESCRIPTION
    1. Checks that kubectl talks to the Docker Desktop cluster.
    2. Creates the "productapi" namespace and the database Secrets (random password, never stored in Git).
    3. Installs Argo CD (pinned version) and lowers its Git polling interval to 60 seconds.
    4. Registers the ProductApi application, after which Argo CD deploys k8s/app from GitHub.

    Safe to run again: existing Secrets are kept, so the database password does not change.
#>
$ErrorActionPreference = 'Stop'

$ArgoCdVersion = 'v3.5.5'
$Namespace = 'productapi'
$RepoRoot = Split-Path -Parent $PSScriptRoot

function Invoke-Kubectl {
    & kubectl @args
    if ($LASTEXITCODE -ne 0) { throw "kubectl $($args -join ' ') failed with exit code $LASTEXITCODE" }
}

function New-RandomPassword {
    $bytes = New-Object byte[] 24
    [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
    return ($bytes | ForEach-Object { $_.ToString('x2') }) -join ''
}

Write-Host '==> Checking the Kubernetes context' -ForegroundColor Cyan
$context = (& kubectl config current-context 2>$null)
if ($context -ne 'docker-desktop') {
    throw "Current kubectl context is '$context'. Enable Kubernetes in Docker Desktop and run: kubectl config use-context docker-desktop"
}
Invoke-Kubectl get nodes

Write-Host "==> Creating namespace '$Namespace' and database Secrets" -ForegroundColor Cyan
& kubectl create namespace $Namespace --dry-run=client -o yaml | kubectl apply -f -

& kubectl get secret postgres-credentials -n $Namespace *> $null
if ($LASTEXITCODE -eq 0) {
    Write-Host 'Secrets already exist, keeping the current database password.'
}
else {
    $password = New-RandomPassword
    $connectionString = "Host=postgres;Port=5432;Database=products;Username=productapi;Password=$password"
    Invoke-Kubectl create secret generic postgres-credentials -n $Namespace "--from-literal=password=$password"
    Invoke-Kubectl create secret generic productapi-database -n $Namespace "--from-literal=connection-string=$connectionString"
}

Write-Host "==> Installing Argo CD $ArgoCdVersion" -ForegroundColor Cyan
& kubectl create namespace argocd --dry-run=client -o yaml | kubectl apply -f -
# Server-side apply: the Argo CD CRDs are too large for a client-side apply annotation.
Invoke-Kubectl apply -n argocd --server-side --force-conflicts -f "https://raw.githubusercontent.com/argoproj/argo-cd/$ArgoCdVersion/manifests/install.yaml"

# Poll Git every 60 seconds instead of the default 3 minutes, so a deploy shows up quickly in the demo.
Invoke-Kubectl patch configmap argocd-cm -n argocd --type merge --patch-file (Join-Path $RepoRoot 'k8s/argocd/argocd-cm-patch.yaml')
Invoke-Kubectl rollout restart statefulset argocd-application-controller -n argocd
Invoke-Kubectl rollout status deployment argocd-server -n argocd --timeout=300s
Invoke-Kubectl rollout status statefulset argocd-application-controller -n argocd --timeout=300s

Write-Host '==> Registering the ProductApi application' -ForegroundColor Cyan
Invoke-Kubectl apply -f (Join-Path $RepoRoot 'k8s/argocd/application.yaml')

$encoded = & kubectl get secret argocd-initial-admin-secret -n argocd -o jsonpath='{.data.password}'
$adminPassword = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($encoded))

Write-Host ''
Write-Host 'Done. Argo CD is now deploying the ProductApi from GitHub.' -ForegroundColor Green
Write-Host ''
Write-Host 'Argo CD UI:   kubectl port-forward svc/argocd-server -n argocd 8443:443'
Write-Host '              then open https://localhost:8443 (accept the self-signed certificate)'
Write-Host "              user: admin   password: $adminPassword"
Write-Host 'ProductApi:   http://localhost:8090  (after Argo CD has synced, about 1-2 minutes)'
Write-Host "Status:       kubectl get pods -n $Namespace"
