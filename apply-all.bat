@echo off
echo Aplicando arquivos YAML de todos os serviços...

REM Caminho base (ajuste se necessário)
set BASE_PATH=k8s

REM Lista de subpastas dos serviços
set SERVICES=^
mpcalc-register-producer ^
mpcalc-deleter-producer ^
mpcalc-updater-producer ^
mpcalc-hub-personal ^
mpcalc-hub-delete ^
mpazure-functions ^
rabbitmq ^
grafana ^
prometheus ^
kong ^
konga

REM Loop pelos serviços
for %%S in (%SERVICES%) do (
    echo Aplicando: %%S
    kubectl apply -f %BASE_PATH%\%%S\deployment.yaml
    kubectl apply -f %BASE_PATH%\%%S\service.yaml
    echo.
)

echo Todos os serviços foram aplicados ao cluster Kubernetes.
pause
