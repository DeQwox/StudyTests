Vagrant.configure("2") do |config|
  config.vm.box_check_update = false

  # -------- Ubuntu VM --------
  config.vm.define "ubuntu" do |ubuntu|
    ubuntu.vm.box = "ubuntu/jammy64"
    ubuntu.vm.hostname = "ubuntu-vm"
    ubuntu.vm.network "private_network", ip: "192.168.56.10"

    ubuntu.vm.provision "shell", inline: <<-SHELL
      echo "=== Ubuntu: installing dependencies ==="
      sudo apt-get update -y
      sudo apt-get install -y wget unzip git apt-transport-https

      # Install Microsoft package repo
      wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
      sudo dpkg -i packages-microsoft-prod.deb
      rm packages-microsoft-prod.deb

      sudo apt-get update -y
      sudo apt-get install -y dotnet-sdk-9.0

      echo "=== Downloading packaged application archive ==="
      wget -O /home/vagrant/StudyTests.zip https://raw.githubusercontent.com/DeQwox/StudyTests/main/StudyTests.zip

      echo "=== Unpacking application ==="
      cd /home/vagrant
      unzip StudyTests.zip -d StudyTests

      echo "=== Building and running application ==="
      cd StudyTests
      dotnet build -c Release
      nohup dotnet run -c Release > app.log 2>&1 &

      echo "Ubuntu: Application deployed and running. Check /home/vagrant/StudyTests/app.log"
    SHELL
  end

  # -------- Debian VM --------
  config.vm.define "debian" do |debian|
    debian.vm.box = "debian/bookworm64"
    debian.vm.hostname = "debian-vm"
    debian.vm.network "private_network", ip: "192.168.56.11"

    debian.vm.provision "shell", inline: <<-SHELL
      echo "=== Debian: installing dependencies ==="
      sudo apt-get update -y
      sudo apt-get install -y wget unzip git apt-transport-https

      # Install Microsoft package repo
      wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
      sudo dpkg -i packages-microsoft-prod.deb
      rm packages-microsoft-prod.deb

      sudo apt-get update -y
      sudo apt-get install -y dotnet-sdk-9.0

      echo "=== Downloading packaged application archive ==="
      wget -O /home/vagrant/StudyTests.zip https://raw.githubusercontent.com/DeQwox/StudyTests/main/StudyTests.zip

      echo "=== Unpacking application ==="
      cd /home/vagrant
      unzip StudyTests.zip -d StudyTests

      echo "=== Building and running application ==="
      cd StudyTests
      dotnet build -c Release
      nohup dotnet run -c Release > app.log 2>&1 &

      echo "Debian: Application deployed and running. Check /home/vagrant/StudyTests/app.log"
    SHELL
  end
end
