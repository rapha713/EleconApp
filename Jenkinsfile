
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

        stage('Preparar artifacts') {
            steps {
                bat '''
                    if exist artifacts rmdir /s /q artifacts
                    mkdir artifacts
                '''
            }
        }

        stage('Restaurar dependencias') {
            steps {
                bat 'dotnet restore EleconApp.csproj'
            }
        }

        stage('Publicar Android APK') {
            steps {
                withCredentials([
                    file(
                        credentialsId: 'android-keystore',
                        variable: 'ANDROID_KEYSTORE'
                    ),
                    string(
                        credentialsId: 'android-keystore-password',
                        variable: 'ANDROID_KEYSTORE_PASSWORD'
                    )
                ]) {
                    bat '''
                        @echo off

                        dotnet publish EleconApp.csproj ^
                            -f net10.0-android ^
                            -c Release ^
                            -p:AndroidPackageFormats=apk ^
                            -p:AndroidKeyStore=true ^
                            -p:AndroidSigningKeyStore="%ANDROID_KEYSTORE%" ^
                            -p:AndroidSigningKeyAlias=eleconapp ^
                            -p:AndroidSigningKeyPass=env:ANDROID_KEYSTORE_PASSWORD ^
                            -p:AndroidSigningStorePass=env:ANDROID_KEYSTORE_PASSWORD

                        if errorlevel 1 exit /b 1

                        copy /Y "bin\\Release\\net10.0-android\\publish\\*-Signed.apk" "artifacts\\EleconApp.apk"

                        if errorlevel 1 exit /b 1
                    '''
                }
            }
        }

        stage('Publicar Windows Unpackaged') {
            steps {
                bat '''
                    dotnet publish EleconApp.csproj ^
                        -f net10.0-windows10.0.19041.0 ^
                        -c Release ^
                        -p:RuntimeIdentifierOverride=win-x64 ^
                        -p:WindowsPackageType=None ^
                        -p:WindowsAppSDKSelfContained=true ^
                        -p:SelfContained=true

                    if errorlevel 1 exit /b 1
                '''

                bat '''
                    powershell -NoProfile -ExecutionPolicy Bypass -Command "Compress-Archive -Path 'bin\\Release\\net10.0-windows10.0.19041.0\\win-x64\\publish\\*' -DestinationPath 'artifacts\\EleconApp-Windows-Unpackaged.zip' -Force"

                    if errorlevel 1 exit /b 1
                '''
            }
        }

        stage('Publicar Windows MSIX') {
            steps {
                bat '''
                    dotnet publish EleconApp.csproj ^
                        -f net10.0-windows10.0.19041.0 ^
                        -c Release ^
                        -p:RuntimeIdentifierOverride=win-x64 ^
                        -p:WindowsPackageType=MSIX ^
                        -p:AppxPackageSigningEnabled=true ^
                        -p:PackageCertificateThumbprint=72B1001D43B2ECF0628C02BD67928F572A557AE4

                    if errorlevel 1 exit /b 1
                '''

                bat '''
                    powershell -NoProfile -ExecutionPolicy Bypass -Command "$p = Get-ChildItem 'bin\\Release\\net10.0-windows10.0.19041.0\\win-x64\\AppPackages' -Filter '*.msix' -Recurse | Select-Object -First 1; if (-not $p) { throw 'Arquivo MSIX nao encontrado'; }; Copy-Item $p.FullName 'artifacts\\EleconApp.msix' -Force"

                    if errorlevel 1 exit /b 1
                '''
            }
        }

        stage('Arquivar entregaveis') {
            steps {
                archiveArtifacts(
                    artifacts: 'artifacts/*',
                    fingerprint: true,
                    allowEmptyArchive: false
                )
            }
        }
    }
}
