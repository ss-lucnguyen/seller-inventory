# Seller Inventory - Manual Deployment Guide

This guide walks you through deploying the Seller Inventory application to Google Cloud Run.

## Prerequisites

1. **Google Cloud CLI (gcloud)** - Install from https://cloud.google.com/sdk/docs/install
2. **Docker Desktop** - Install and ensure it's running
3. **A GCP Project** - Create one at https://console.cloud.google.com
4. **NEON PostgreSQL Database** - Free tier at https://console.neon.tech

## Quick Deploy (Automated)

```bash
# Set your project ID and run
GCP_PROJECT_ID=your-project-id ./deploy-gcloud.sh deploy-all
```

## Manual Step-by-Step Deployment

### Step 1: Authenticate with Google Cloud

```bash
# Login to GCP
gcloud auth login

# Set your project
gcloud config set project YOUR_PROJECT_ID

# Configure Docker for GCR
gcloud auth configure-docker
```

### Step 2: Enable Required APIs

```bash
gcloud services enable \
    cloudbuild.googleapis.com \
    run.googleapis.com \
    containerregistry.googleapis.com \
    secretmanager.googleapis.com \
    storage.googleapis.com
```

### Step 3: Create Cloud Storage Bucket (for product images)

```bash
# Create bucket (use US multi-region for free tier)
gsutil mb -p YOUR_PROJECT_ID -l US gs://sellerinventory-images

# Set CORS policy
cat > /tmp/cors.json << 'EOF'
[
  {
    "origin": ["*"],
    "method": ["GET", "HEAD"],
    "responseHeader": ["Content-Type"],
    "maxAgeSeconds": 3600
  }
]
EOF
gsutil cors set /tmp/cors.json gs://sellerinventory-images

# Make bucket publicly readable
gsutil iam ch allUsers:objectViewer gs://sellerinventory-images
```

### Step 4: Create Secrets in Secret Manager

```bash
# Create JWT secret
gcloud secrets create jwt-secret-key --replication-policy="automatic"
echo -n 'YourSuperSecretKeyThatIsAtLeast32CharactersLong!' | \
    gcloud secrets versions add jwt-secret-key --data-file=-

# Create database connection string secret
gcloud secrets create database-url --replication-policy="automatic"
echo -n 'Host=ep-xxx.region.aws.neon.tech;Port=5432;Database=neondb;Username=user;Password=pass;SSL Mode=Require' | \
    gcloud secrets versions add database-url --data-file=-
```

> **Note:** Replace the database connection string with your actual NEON PostgreSQL connection details from https://console.neon.tech

### Step 5: Build and Push API Docker Image

```bash
# From the project root directory
docker build --platform linux/amd64 -f src/Api/Dockerfile -t gcr.io/YOUR_PROJECT_ID/sellerinventory-api:latest .

# Push to Google Container Registry
docker push gcr.io/YOUR_PROJECT_ID/sellerinventory-api:latest
```

### Step 6: Deploy API to Cloud Run

```bash
gcloud run deploy sellerinventory-api \
    --image gcr.io/YOUR_PROJECT_ID/sellerinventory-api:latest \
    --platform managed \
    --region us-east1 \
    --allow-unauthenticated \
    --port 8080 \
    --memory 256Mi \
    --cpu 1 \
    --min-instances 0 \
    --max-instances 2 \
    --set-env-vars "ASPNETCORE_ENVIRONMENT=Production" \
    --set-env-vars "StorageProvider=GoogleCloud" \
    --set-env-vars "GoogleCloud__BucketName=sellerinventory-images" \
    --set-env-vars "JwtSettings__Issuer=SellerInventory" \
    --set-env-vars "JwtSettings__Audience=SellerInventory" \
    --set-env-vars "JwtSettings__ExpirationHours=24" \
    --set-secrets "ConnectionStrings__DefaultConnection=database-url:latest" \
    --set-secrets "JwtSettings__SecretKey=jwt-secret-key:latest"

# Get the API URL
API_URL=$(gcloud run services describe sellerinventory-api --region us-east1 --format="value(status.url)")
echo "API URL: $API_URL"
```

### Step 7: Build and Push Web Docker Image

```bash
# Build with the API URL from previous step
docker build \
    --platform linux/amd64 \
    -f src/Client/BlazorWeb/Dockerfile \
    --build-arg API_BASE_URL=$API_URL \
    -t gcr.io/YOUR_PROJECT_ID/sellerinventory-web:latest \
    .

# Push to GCR
docker push gcr.io/YOUR_PROJECT_ID/sellerinventory-web:latest
```

### Step 8: Deploy Web to Cloud Run

```bash
gcloud run deploy sellerinventory-web \
    --image gcr.io/YOUR_PROJECT_ID/sellerinventory-web:latest \
    --platform managed \
    --region us-east1 \
    --allow-unauthenticated \
    --port 80 \
    --memory 128Mi \
    --cpu 1 \
    --min-instances 0 \
    --max-instances 2

# Get the Web URL
WEB_URL=$(gcloud run services describe sellerinventory-web --region us-east1 --format="value(status.url)")
echo "Web URL: $WEB_URL"
```

### Step 9: Update API CORS Settings

```bash
# Update API to allow requests from the Web URL
gcloud run services update sellerinventory-api \
    --region us-east1 \
    --update-env-vars "AllowedOrigins__0=$WEB_URL"
```

## Verification

1. Open the Web URL in your browser
2. Login with default credentials:
   - SystemAdmin: `sysadmin` / `SysAdmin@123`
   - Manager: `manager` / `Manager@123`
   - Staff: `staff` / `Staff@123`

## Check Deployment Status

```bash
# View running services
gcloud run services list --region us-east1

# View API logs
gcloud run services logs read sellerinventory-api --region us-east1

# View Web logs
gcloud run services logs read sellerinventory-web --region us-east1
```

## Environment Variables Reference

### API Service

| Variable | Description |
|----------|-------------|
| `ASPNETCORE_ENVIRONMENT` | Set to `Production` |
| `StorageProvider` | `GoogleCloud` or `Local` |
| `GoogleCloud__BucketName` | GCS bucket name for images |
| `JwtSettings__Issuer` | JWT issuer name |
| `JwtSettings__Audience` | JWT audience name |
| `JwtSettings__ExpirationHours` | JWT token expiry (hours) |
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string (from Secret Manager) |
| `JwtSettings__SecretKey` | JWT signing key (from Secret Manager) |
| `AllowedOrigins__0` | CORS allowed origin (Web URL) |

## Free Tier Limits

| Service | Free Tier |
|---------|-----------|
| Cloud Run | 2M requests/month, 360K GB-seconds, 180K vCPU-seconds |
| Cloud Storage | 5GB storage, 1GB egress |
| Secret Manager | 6 active secret versions |
| Container Registry | 0.5GB storage |

> **Tip:** Use regions `us-central1`, `us-east1`, or `us-west1` for Cloud Run free tier.

## Troubleshooting

### Docker not running
```bash
# macOS - Start Docker Desktop from Applications
open -a Docker
```

### Authentication issues
```bash
gcloud auth login
gcloud auth configure-docker
```

### Database connection failed
- Verify your NEON connection string includes `SSL Mode=Require`
- Check the secret value: `gcloud secrets versions access latest --secret=database-url`

### CORS errors in browser
- Ensure `AllowedOrigins__0` is set to the exact Web URL (including `https://`)
- Redeploy API after updating CORS settings

### View container logs
```bash
gcloud run services logs read sellerinventory-api --region us-east1 --limit 50
```
