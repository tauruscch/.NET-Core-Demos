Param(
    [parameter(Mandatory=$true)][string]$tag
)


docker build --no-cache  -f  .\microservices\Services\Ordering\GeekTime.Ordering.API\Dockerfile -t geektime-ordering-api:$tag .
docker build --no-cache  -f  .\microservices\Services\Identity\GeekTime.Identity.API\Dockerfile -t geektime-identity-api:$tag .
docker build --no-cache  -f  .\microservices\ApiGateways\GeekTime.Mobile.Gateway\Dockerfile -t geektime-mobile-gateway:$tag .
docker build --no-cache  -f  .\microservices\ApiGateways\GeekTime.Mobile.ApiAggregator\Dockerfile -t geektime-mobile-apiaggregator:$tag .
docker build --no-cache  -f  .\microservices\Monitor\GeekTime.HealthChecksHost\Dockerfile -t geektime-healthcheckshost:$tag .

"Any key to exit"  ;
Read-Host | Out-Null ;
Exit