# VoiceScribe Makefile

# Configuration
APP_NAME := voicescribe
PUBLISH_APP := VoiceScribe
CONFIGURATION := Release
RUNTIME := linux-x64
PUBLISH_DIR := bin/publish
INSTALL_DIR := /usr/local/bin

# .NET commands
DOTNET := dotnet

.PHONY: all build test clean install uninstall publish

# Default target
all: build

# Build the application
build:
	$(DOTNET) build -c $(CONFIGURATION)

# Run the application
run:
	$(DOTNET) run -c $(CONFIGURATION)

# Run tests
test:
	$(DOTNET) test -c $(CONFIGURATION)

# Publish self-contained executable
publish:
	$(DOTNET) publish VoiceScribe.csproj -c $(CONFIGURATION) -r $(RUNTIME) --self-contained true /p:PublishSingleFile=true -o $(PUBLISH_DIR)

# Install to /usr/local/bin (requires sudo)
install: publish
	@mkdir -p $(INSTALL_DIR)
	@cp $(PUBLISH_DIR)/$(PUBLISH_APP) $(INSTALL_DIR)/$(APP_NAME)
	@chmod +x $(INSTALL_DIR)/$(APP_NAME)
	@echo "Installed $(APP_NAME) to $(INSTALL_DIR)"
	@echo "Make sure $(INSTALL_DIR) is in your PATH"

# Uninstall from /usr/local/bin (requires sudo)
uninstall:
	@rm -f $(INSTALL_DIR)/$(APP_NAME)
	@echo "Uninstalled $(APP_NAME) from $(INSTALL_DIR)"

# Clean build artifacts
clean:
	$(DOTNET) clean -c $(CONFIGURATION)
	@rm -rf $(PUBLISH_DIR)
	@rm -rf bin/
	@rm -rf obj/

# Help
help:
	@echo "Available targets:"
	@echo "  build     - Build the application (default)"
	@echo "  test      - Run tests"
	@echo "  publish   - Publish self-contained executable"
	@echo "  install   - Install to /usr/local/bin (requires sudo)"
	@echo "  uninstall - Remove from /usr/local/bin (requires sudo)"
	@echo "  clean     - Clean build artifacts"
	@echo ""
	@echo "Configuration:"
	@echo "  INSTALL_DIR=$(INSTALL_DIR)"
	@echo "  CONFIGURATION=$(CONFIGURATION)"
