#
# setup_ai_agent.ps1
# Download application insights agent
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$url = 'https://github.com/microsoft/ApplicationInsights-Java/releases/download/2.3.1/applicationinsights-agent-2.3.1.jar'
Invoke-WebRequest -Uri $url -OutFile .\applicationinsights-agent-2.3.1.jar
