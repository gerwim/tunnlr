# Self host Tunnlr

Tunnlr is fully self hostable.

Requirements:
* DNS record for your API
* Wildcard DNS record for the tunnel URL

## Setup

If you use the binaries from the [releases](https://github.com/gerwim/tunnlr/releases) page, make sure you use the same version (for client and server).

### Client
Edit `asppsettings.json` and change the key `Tunnlr:Client:ApiServer` to your API server.

### API and proxy server
There is a prebuild Docker image available. The Docker compose file is the file used on api.tunnlr.dev. This file includes both the reverse proxy (Caddy) and the Tunnlr proxy. If your deployment needs differ, make sure the GRPC port (5107) is available on Http2 and the web port (5108) with Http 1.


Make sure you change the values regarding URL's before deployment.
```
version: "3"
services:
  caddy:
    image: gerwim/caddy-docker-proxy-cloudflare:2.7.5
    ports:
      - 80:80
      - 443:443
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock
      - caddy_data:/data
    restart: always
  tunnlr-proxy:
    image: ghcr.io/gerwim/tunnlr
    environment:
      - Tunnlr__Server__Proxy__ServedFromWildcard=https://*.tunnel.tunnlr.dev
      - Tunnlr__Server__Persistence__Provider=sqlite # supports either 'sqlite' or 'postgresql'
      - Tunnlr__Server__Persistence__ConnectionString=Data Source=App_Data/Tunnlr.Server.db
    volumes:
      - tunnlr_data:/app/App_Data
    restart: always
    labels:
      caddy_0: api.tunnlr.dev
      caddy_0.reverse_proxy: "{{upstreams h2c 5107}}"
      caddy_1: "*.tunnel.tunnlr.dev"
      caddy_1.reverse_proxy: "{{upstreams http 5108}}"
      caddy_1.tls.dns: cloudflare REDACTED

volumes:
  caddy_data: {}
  tunnlr_data: {}
```