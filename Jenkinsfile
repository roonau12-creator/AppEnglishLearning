pipeline {
    agent any

    options {
        timestamps()
        disableConcurrentBuilds()
        buildDiscarder(logRotator(numToKeepStr: '15'))
    }

    environment {
        // SMTP, DB, OAuth: inject at deploy time (Jenkins credentials), never from git.
        DOTNET_CLI_TELEMETRY_OPTOUT = '1'
        DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
        DOTNET_NOLOGO = '1'
        PROJECT = 'AppLearningEnglish/AppLearningEnglish.csproj'
        CONFIGURATION = 'Release'
        PUBLISH_DIR = 'publish'
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Restore') {
            steps {
                sh 'dotnet --info'
                sh "dotnet restore ${PROJECT}"
            }
        }

        stage('Build') {
            steps {
                sh "dotnet build ${PROJECT} --no-restore -c ${CONFIGURATION}"
            }
        }

        stage('Test') {
            steps {
                echo 'Chưa có project test. Bỏ qua stage Test.'
            }
        }

        stage('Publish') {
            steps {
                sh """
                    rm -rf ${PUBLISH_DIR}
                    dotnet publish ${PROJECT} --no-build -c ${CONFIGURATION} -o ${PUBLISH_DIR}
                """
                archiveArtifacts artifacts: "${PUBLISH_DIR}/**", fingerprint: true
            }
        }
    }

    post {
        success {
            echo "Build ${env.BUILD_NUMBER} thành công. Artifact nằm trong ${PUBLISH_DIR}/"
        }
        failure {
            echo "Build ${env.BUILD_NUMBER} thất bại. Xem log stage Restore/Build."
        }
        cleanup {
            cleanWs(deleteDirs: true, notFailBuild: true)
        }
    }
}
