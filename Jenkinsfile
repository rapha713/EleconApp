
pipeline {
    agent {
        label 'windows'
    }

    options {
        timestamps()
    }

    stages {

        stage('Verificar ambiente') {
            steps {
                bat 'dotnet --version'
                bat 'git --version'
            }
        }

        stage('Restaurar dependencias') {
            steps {
                bat 'dotnet restore EleconApp.csproj'
            }
        }

        stage('Compilar Android') {
            steps {
                bat 'dotnet build EleconApp.csproj --configuration Release --framework net10.0-android --no-restore'
            }
        }

        stage('Compilar Windows') {
            steps {
                bat 'dotnet build EleconApp.csproj --configuration Release --framework net10.0-windows10.0.19041.0 --no-restore'
            }
        }
    }
}
