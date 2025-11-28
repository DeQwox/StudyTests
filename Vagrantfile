# -*- mode: ruby -*-
# vi: set ft=ruby :

Vagrant.configure("2") do |config|
  # Ubuntu VM configuration
  config.vm.define "ubuntu" do |ubuntu|
    ubuntu.vm.box = "ubuntu/jammy64"
    ubuntu.vm.hostname = "ubuntu-vm"
    ubuntu.vm.network "private_network", type: "dhcp"
    
    ubuntu.vm.provision "shell", inline: <<-SHELL
      sudo apt-get update
      sudo apt-get install -y wget unzip
      wget -O /home/vagrant/StudyTests.zip https://raw.githubusercontent.com/DeQwox/StudyTests/main/StudyTests.zip
      chown vagrant:vagrant /home/vagrant/StudyTests.zip
    SHELL

    ubuntu.vm.provision "shell", inline: <<-SHELL
      sudo apt-get update
      sudo apt-get install -y dotnet-sdk-9.0
    SHELL

    ubuntu.vm.provider "virtualbox" do |vb|
      vb.memory = "2048"
      vb.cpus = 2
    end
  end
# Windows VM configuration
  config.vm.define "windows" do |windows|
    windows.vm.box = "gusztavvargadr/windows-11"
    windows.vm.hostname = "windows-vm"
    windows.vm.network "private_network", type: "dhcp"

    # We explicitly tell Vagrant this is a PowerShell script
    windows.vm.provision "shell", inline: <<-SHELL
      $ErrorActionPreference = "Stop"

      Write-Host "=== Downloading .NET SDK installer ==="
      $dotnetUrl = "https://builds.dotnet.microsoft.com/dotnet/Sdk/9.0.305/dotnet-sdk-9.0.305-win-x64.exe"
      $dotnetInstaller = "C:\\dotnet-sdk.exe"
      Invoke-WebRequest -Uri $dotnetUrl -OutFile $dotnetInstaller

      Write-Host "=== Installing .NET SDK ==="
      Start-Process -FilePath $dotnetInstaller -ArgumentList "/quiet" -Wait
      
      # Update the PATH for the CURRENT session so the next commands work
      Write-Host "=== Updating PATH ==="
      $env:PATH = "$env:PATH;C:\Program Files\dotnet"
      
      # Verify dotnet is working
      dotnet --version

      # Download and Unzip Project
      Write-Host "=== Setting up Project ==="
      Invoke-WebRequest -Uri 'https://raw.githubusercontent.com/DeQwox/StudyTests/main/StudyTests.zip' -OutFile 'C:\\Users\\vagrant\\StudyTests.zip'
      Expand-Archive -Path 'C:\\Users\\vagrant\\StudyTests.zip' -DestinationPath 'C:\\Users\\vagrant\\StudyTests' -Force
      Remove-Item 'C:\\Users\\vagrant\\StudyTests.zip'

      # Navigate and Setup Project
      Set-Location "C:\\Users\\vagrant\\StudyTests\\Tests\\StudyTests.Tests"
      
      dotnet restore
      dotnet build
    SHELL

    windows.vm.provider "virtualbox" do |vb|
      vb.memory = "4096"
      vb.cpus = 2
      vb.gui = true
    end

    windows.vm.communicator = "winrm"
    windows.winrm.username = "vagrant"
    windows.winrm.password = "vagrant"
  end
end