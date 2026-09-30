# How to get running
1) fill out the .env file:
   - POSTGRES_PASSWORD: just write something random. if you change it later, you'll have to delete the docker volume.
   - TRUSTED_DOMAIN: the allowed domain of target urls
2) assuming you have docker installed, run the command:
```
docker compose up
```
this should bring up the following services:
- localhost:18888 -> Aspire dashboard, to monitor logs
- localhost:8080/scalar/v1 -> API documentation and testing
- localhost:8080 -> API root

# Code formats
`POST /shorten` takes an optional `format`:
- `numeric` (default): 7 digit random code
- `alphanum`: alphanumeric code of minimum length 5


# Target urls
`POST /shorten` accepts:
- relative paths, e.g. `/some/page` (redirects stay on the host serving the short url)
- absolute `http`/`https` urls on `TRUSTED_DOMAIN` or its subdomains

Anything else returns 400. If `TRUSTED_DOMAIN` is empty, only relative paths are accepted.


# How to get coding
install dotnet sdk and aspire CLI  
'aspire run' will bring everything up  
'aspire publish -o .' in the root will recreate/update Dockerfile and compose file  