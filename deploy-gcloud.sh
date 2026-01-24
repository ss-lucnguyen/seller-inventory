#!/bin/bash
set -e

# =====================================================
# Google Cloud Deployment Script for Seller Inventory
# Deploys API and Blazor Web to Cloud Run (FREE TIER)
# Uses: NEON PostgreSQL (Free), Google Cloud Storage (Free)
# =====================================================

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

print_step() {
    echo -e "${GREEN}==> $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}Warning: $1${NC}"
}

print_error() {
    echo -e "${RED}Error: $1${NC}"
}

# =====================================================
# Configuration - Update these values
# =====================================================
GCP_PROJECT_ID="${GCP_PROJECT_ID:-your-gcp-project-id}"
GCP_REGION="${GCP_REGION:-us-central1}"  # FREE TIER: us-central1, us-east1, us-west1 only
GCS_BUCKET_NAME="${GCS_BUCKET_NAME:-sellerinventory-images}"

# Service names
API_SERVICE_NAME="sellerinventory-api"
WEB_SERVICE_NAME="sellerinventory-web"

# Image tags
TAG="${TAG:-latest}"
API_IMAGE="gcr.io/${GCP_PROJECT_ID}/${API_SERVICE_NAME}:${TAG}"
WEB_IMAGE="gcr.io/${GCP_PROJECT_ID}/${WEB_SERVICE_NAME}:${TAG}"

# =====================================================
# Pre-flight checks
# =====================================================
check_prerequisites() {
    print_step "Checking prerequisites..."

    # Check gcloud CLI
    if ! command -v gcloud &> /dev/null; then
        print_error "gcloud CLI is not installed. Please install it from https://cloud.google.com/sdk/docs/install"
        exit 1
    fi

    # Check docker
    if ! command -v docker &> /dev/null; then
        print_error "Docker is not installed. Please install Docker."
        exit 1
    fi

    # Check if authenticated
    if ! gcloud auth list --filter=status:ACTIVE --format="value(account)" | head -n1 &> /dev/null; then
        print_error "Not authenticated with gcloud. Run: gcloud auth login"
        exit 1
    fi

    echo "All prerequisites met!"
}

# =====================================================
# Setup GCP Project
# =====================================================
setup_gcp() {
    print_step "Setting up GCP project: ${GCP_PROJECT_ID}"

    # Set project
    gcloud config set project ${GCP_PROJECT_ID}

    # Enable required APIs
    print_step "Enabling required APIs..."
    gcloud services enable \
        cloudbuild.googleapis.com \
        run.googleapis.com \
        containerregistry.googleapis.com \
        secretmanager.googleapis.com \
        sqladmin.googleapis.com \
        storage.googleapis.com

    echo "GCP setup complete!"
}

# =====================================================
# Create Cloud Storage Bucket
# =====================================================
create_storage_bucket() {
    print_step "Creating Cloud Storage bucket: ${GCS_BUCKET_NAME}"

    # Check if bucket exists
    if gsutil ls -b gs://${GCS_BUCKET_NAME} &> /dev/null; then
        echo "Bucket ${GCS_BUCKET_NAME} already exists"
    else
        # Create bucket in US multi-region for free tier (5GB free)
        gsutil mb -p ${GCP_PROJECT_ID} -l US gs://${GCS_BUCKET_NAME}

        # Set CORS for the bucket (allows browser access)
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
        gsutil cors set /tmp/cors.json gs://${GCS_BUCKET_NAME}
        rm /tmp/cors.json

        # Make bucket publicly readable (for product images)
        gsutil iam ch allUsers:objectViewer gs://${GCS_BUCKET_NAME}

        echo "Bucket ${GCS_BUCKET_NAME} created and configured"
    fi
}

# =====================================================
# Build and Push Docker Images
# =====================================================
build_and_push_images() {
    print_step "Building and pushing Docker images..."

    # Configure Docker for GCR
    gcloud auth configure-docker --quiet

    # Build API image
    print_step "Building API image: ${API_IMAGE}"
    docker build \
        -f src/Api/Dockerfile \
        -t ${API_IMAGE} \
        .

    # Push API image
    print_step "Pushing API image..."
    docker push ${API_IMAGE}

    # Get API URL (will be available after first deploy, use placeholder for now)
    API_URL="${API_URL:-https://${API_SERVICE_NAME}-${GCP_PROJECT_ID}.${GCP_REGION}.run.app}"

    # Build Web image with API URL
    print_step "Building Web image: ${WEB_IMAGE}"
    docker build \
        -f src/Client/BlazorWeb/Dockerfile \
        --build-arg API_BASE_URL=${API_URL} \
        -t ${WEB_IMAGE} \
        .

    # Push Web image
    print_step "Pushing Web image..."
    docker push ${WEB_IMAGE}

    echo "Docker images built and pushed successfully!"
}

# =====================================================
# Create Secrets in Secret Manager
# =====================================================
create_secrets() {
    print_step "Setting up secrets in Secret Manager..."

    # Check if secrets exist and create if not
    secrets=("jwt-secret-key" "database-url")

    for secret in "${secrets[@]}"; do
        if ! gcloud secrets describe ${secret} &> /dev/null; then
            echo "Creating secret: ${secret}"
            gcloud secrets create ${secret} --replication-policy="automatic"

            if [ "$secret" == "jwt-secret-key" ]; then
                echo -e "\n${YELLOW}Please add JWT secret key:${NC}"
                echo "Run: echo -n 'YourSuperSecretKeyThatIsAtLeast32CharactersLong!' | gcloud secrets versions add jwt-secret-key --data-file=-"
            elif [ "$secret" == "database-url" ]; then
                echo -e "\n${YELLOW}Please add NEON PostgreSQL connection string:${NC}"
                echo "Get your connection string from: https://console.neon.tech"
                echo "Run: echo -n 'Host=ep-xxx.region.aws.neon.tech;Port=5432;Database=neondb;Username=user;Password=pass;SSL Mode=Require' | gcloud secrets versions add database-url --data-file=-"
            fi
        else
            echo "Secret ${secret} already exists"
        fi
    done
}

# =====================================================
# Deploy API to Cloud Run
# =====================================================
deploy_api() {
    print_step "Deploying API to Cloud Run..."

    # Get service account email
    SERVICE_ACCOUNT=$(gcloud iam service-accounts list --filter="displayName:Compute Engine default" --format="value(email)" | head -n1)

    if [ -z "$SERVICE_ACCOUNT" ]; then
        SERVICE_ACCOUNT="${GCP_PROJECT_ID}@appspot.gserviceaccount.com"
    fi

    gcloud run deploy ${API_SERVICE_NAME} \
        --image ${API_IMAGE} \
        --platform managed \
        --region ${GCP_REGION} \
        --allow-unauthenticated \
        --port 8080 \
        --memory 256Mi \
        --cpu 1 \
        --min-instances 0 \
        --max-instances 2 \
        --set-env-vars "ASPNETCORE_ENVIRONMENT=Production" \
        --set-env-vars "StorageProvider=GoogleCloud" \
        --set-env-vars "GoogleCloud__BucketName=${GCS_BUCKET_NAME}" \
        --set-env-vars "JwtSettings__Issuer=SellerInventory" \
        --set-env-vars "JwtSettings__Audience=SellerInventory" \
        --set-env-vars "JwtSettings__ExpirationHours=24" \
        --set-secrets "ConnectionStrings__DefaultConnection=database-url:latest" \
        --set-secrets "JwtSettings__SecretKey=jwt-secret-key:latest" \
        --service-account ${SERVICE_ACCOUNT}

    # Get API URL
    API_URL=$(gcloud run services describe ${API_SERVICE_NAME} --region ${GCP_REGION} --format="value(status.url)")

    echo -e "${GREEN}API deployed successfully!${NC}"
    echo "API URL: ${API_URL}"

    # Update CORS for the API
    print_step "Updating API with CORS settings..."
    WEB_URL="${WEB_URL:-https://${WEB_SERVICE_NAME}-${GCP_PROJECT_ID}.${GCP_REGION}.run.app}"

    gcloud run services update ${API_SERVICE_NAME} \
        --region ${GCP_REGION} \
        --set-env-vars "AllowedOrigins__0=${WEB_URL}"
}

# =====================================================
# Deploy Web to Cloud Run
# =====================================================
deploy_web() {
    print_step "Deploying Web to Cloud Run..."

    # Get API URL first
    API_URL=$(gcloud run services describe ${API_SERVICE_NAME} --region ${GCP_REGION} --format="value(status.url)" 2>/dev/null || echo "")

    if [ -z "$API_URL" ]; then
        print_warning "API service not found. Please deploy API first or set API_URL manually."
        API_URL="${API_URL:-https://${API_SERVICE_NAME}-${GCP_PROJECT_ID}.${GCP_REGION}.run.app}"
    fi

    # Rebuild web image with correct API URL
    print_step "Rebuilding Web image with API URL: ${API_URL}"
    docker build \
        -f src/Client/BlazorWeb/Dockerfile \
        --build-arg API_BASE_URL=${API_URL} \
        -t ${WEB_IMAGE} \
        .
    docker push ${WEB_IMAGE}

    gcloud run deploy ${WEB_SERVICE_NAME} \
        --image ${WEB_IMAGE} \
        --platform managed \
        --region ${GCP_REGION} \
        --allow-unauthenticated \
        --port 80 \
        --memory 128Mi \
        --cpu 1 \
        --min-instances 0 \
        --max-instances 2

    # Get Web URL
    WEB_URL=$(gcloud run services describe ${WEB_SERVICE_NAME} --region ${GCP_REGION} --format="value(status.url)")

    echo -e "${GREEN}Web deployed successfully!${NC}"
    echo "Web URL: ${WEB_URL}"

    # Update API CORS with actual Web URL
    print_step "Updating API CORS with Web URL..."
    gcloud run services update ${API_SERVICE_NAME} \
        --region ${GCP_REGION} \
        --update-env-vars "AllowedOrigins__0=${WEB_URL}"
}

# =====================================================
# Print Summary
# =====================================================
print_summary() {
    print_step "Deployment Summary"
    echo "========================================"

    API_URL=$(gcloud run services describe ${API_SERVICE_NAME} --region ${GCP_REGION} --format="value(status.url)" 2>/dev/null || echo "Not deployed")
    WEB_URL=$(gcloud run services describe ${WEB_SERVICE_NAME} --region ${GCP_REGION} --format="value(status.url)" 2>/dev/null || echo "Not deployed")

    echo -e "GCP Project:     ${GREEN}${GCP_PROJECT_ID}${NC}"
    echo -e "Region:          ${GREEN}${GCP_REGION}${NC}"
    echo -e "API URL:         ${GREEN}${API_URL}${NC}"
    echo -e "Web URL:         ${GREEN}${WEB_URL}${NC}"
    echo -e "Storage Bucket:  ${GREEN}gs://${GCS_BUCKET_NAME}${NC}"
    echo "========================================"
}

# =====================================================
# Main Script
# =====================================================
show_help() {
    echo "Seller Inventory - Google Cloud Deployment Script"
    echo ""
    echo "Usage: ./deploy-gcloud.sh [command]"
    echo ""
    echo "Commands:"
    echo "  setup       Setup GCP project and enable APIs"
    echo "  storage     Create Cloud Storage bucket"
    echo "  secrets     Create secrets in Secret Manager"
    echo "  build       Build and push Docker images"
    echo "  deploy-api  Deploy API to Cloud Run"
    echo "  deploy-web  Deploy Web to Cloud Run"
    echo "  deploy-all  Run all deployment steps"
    echo "  status      Show deployment status"
    echo "  help        Show this help message"
    echo ""
    echo "Environment variables:"
    echo "  GCP_PROJECT_ID   - GCP project ID (required)"
    echo "  GCP_REGION       - GCP region (default: us-central1)"
    echo "                     FREE TIER regions: us-central1, us-east1, us-west1"
    echo "  GCS_BUCKET_NAME  - Storage bucket name (default: sellerinventory-images)"
    echo "  TAG              - Docker image tag (default: latest)"
    echo ""
    echo "Example:"
    echo "  GCP_PROJECT_ID=my-project ./deploy-gcloud.sh deploy-all"
}

case "${1:-help}" in
    setup)
        check_prerequisites
        setup_gcp
        ;;
    storage)
        check_prerequisites
        create_storage_bucket
        ;;
    secrets)
        check_prerequisites
        create_secrets
        ;;
    build)
        check_prerequisites
        build_and_push_images
        ;;
    deploy-api)
        check_prerequisites
        deploy_api
        ;;
    deploy-web)
        check_prerequisites
        deploy_web
        ;;
    deploy-all)
        check_prerequisites
        setup_gcp
        create_storage_bucket
        create_secrets
        build_and_push_images
        deploy_api
        deploy_web
        print_summary
        ;;
    status)
        print_summary
        ;;
    help|*)
        show_help
        ;;
esac
