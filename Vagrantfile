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

    
    windows.vm.provision "shell", inline: <<-SHELL
      Write-Host "=== Downloading .NET SDK installer ==="
      $dotnetUrl = "https://builds.dotnet.microsoft.com/dotnet/Sdk/9.0.305/dotnet-sdk-9.0.305-win-x64.exe"
      $dotnetInstaller = "C:\\dotnet-sdk.exe"
      Invoke-WebRequest -Uri $dotnetUrl -OutFile $dotnetInstaller

      Write-Host "=== Installing .NET SDK ==="
      Start-Process -FilePath $dotnetInstaller -ArgumentList "/quiet" -Wait
      
      powershell -Command "$env:PATH += ';C:\Program Files\dotnet'"
  
      powershell -Command "Invoke-WebRequest -Uri 'https://raw.githubusercontent.com/DeQwox/StudyTests/main/StudyTests.zip' -OutFile 'C:\\Users\\vagrant\\StudyTests.zip'"
      powershell -Command "Expand-Archive -Path 'C:\\Users\\vagrant\\StudyTests.zip' -DestinationPath 'C:\\Users\\vagrant\\StudyTests' -Force"
      powershell -Command "Remove-Item 'C:\\Users\\vagrant\\StudyTests.zip'"

      powershell -Command "cd C:\\Users\\vagrant\\StudyTests\\Tests\\StudyTests.Tests; dotnet add package xunit --version 2.9.2"
      powershell -Command "cd C:\\Users\\vagrant\\StudyTests\\Tests\\StudyTests.Tests; dotnet add package xunit.runner.visualstudio --version 2.9.2"
      powershell -Command "cd C:\\Users\\vagrant\\StudyTests\\Tests\\StudyTests.Tests; dotnet restore"
      powershell -Command "cd C:\\Users\\vagrant\\StudyTests\\Tests\\StudyTests.Tests; dotnet build"
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