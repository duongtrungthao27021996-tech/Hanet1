#!/bin/bash

# Script deploy Hanet API lên VPS
# Usage: ./deploy.sh [start|stop|restart|logs|update]

set -e

COMPOSE_FILE="docker-compose.yml"
PROJECT_NAME="hanet-api"

function start() {
    echo "🚀 Starting Hanet API..."
    docker-compose -f $COMPOSE_FILE up -d
    echo "✅ Hanet API started successfully!"
    echo "📝 Access Swagger UI at: http://localhost:5000/swagger"
}

function stop() {
    echo "🛑 Stopping Hanet API..."
    docker-compose -f $COMPOSE_FILE down
    echo "✅ Hanet API stopped successfully!"
}

function restart() {
    echo "🔄 Restarting Hanet API..."
    stop
    start
}

function logs() {
    echo "📋 Showing logs..."
    docker-compose -f $COMPOSE_FILE logs -f
}

function update() {
    echo "🔄 Updating Hanet API..."
    
    # Pull latest code (nếu dùng git)
    # git pull origin main
    
    # Rebuild và restart
    echo "🏗️  Building new image..."
    docker-compose -f $COMPOSE_FILE build --no-cache
    
    echo "🛑 Stopping old container..."
    docker-compose -f $COMPOSE_FILE down
    
    echo "🚀 Starting new container..."
    docker-compose -f $COMPOSE_FILE up -d
    
    echo "✅ Update completed successfully!"
    echo "📝 Access Swagger UI at: http://localhost:5000/swagger"
}

function status() {
    echo "📊 Checking status..."
    docker-compose -f $COMPOSE_FILE ps
}

function health() {
    echo "🏥 Checking health..."
    curl -f http://localhost:5000/swagger/index.html || echo "❌ API is not responding"
}

case "$1" in
    start)
        start
        ;;
    stop)
        stop
        ;;
    restart)
        restart
        ;;
    logs)
        logs
        ;;
    update)
        update
        ;;
    status)
        status
        ;;
    health)
        health
        ;;
    *)
        echo "Usage: $0 {start|stop|restart|logs|update|status|health}"
        exit 1
esac
