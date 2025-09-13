#!/bin/bash

# Quick help script for GameSpace
# Usage: ./scripts/quick-help.sh

echo "🚀 GameSpace Quick Help"
echo "======================="
echo

echo "Quick Commands:"
echo "==============="
echo "  Start:     ./scripts/quick-start.sh"
echo "  Stop:      ./scripts/quick-stop.sh"
echo "  Restart:   ./scripts/quick-restart.sh"
echo "  Status:    ./scripts/quick-status.sh"
echo "  Logs:      ./scripts/quick-logs.sh [service] [lines]"
echo "  Monitor:   ./scripts/quick-monitor.sh"
echo "  Backup:    ./scripts/quick-backup.sh"
echo "  Cleanup:   ./scripts/quick-cleanup.sh"
echo "  Help:      ./scripts/quick-help.sh"
echo

echo "Service Options for Logs:"
echo "========================="
echo "  app    - Application logs"
echo "  db     - Database logs"
echo "  redis  - Redis logs"
echo "  nginx  - Nginx logs"
echo "  all    - All logs"
echo

echo "Examples:"
echo "========="
echo "  ./scripts/quick-logs.sh app 50"
echo "  ./scripts/quick-logs.sh db 100"
echo "  ./scripts/quick-logs.sh all 200"
echo

echo "Advanced Management:"
echo "===================="
echo "  For advanced management, use the scripts in the scripts/ directory."
echo "  See scripts/README.md for detailed documentation."
echo

echo "URLs:"
echo "====="
echo "  Application: http://localhost"
echo "  Admin:       http://localhost/admin"
echo "  API Health:  http://localhost/api/health"
echo

echo "Need Help?"
echo "=========="
echo "  Check the logs: ./scripts/quick-logs.sh"
echo "  Check status:   ./scripts/quick-status.sh"
echo "  Monitor:        ./scripts/quick-monitor.sh"