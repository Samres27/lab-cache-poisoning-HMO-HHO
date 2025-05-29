# varnish/default.vcl

vcl 4.1;

acl local_purge_ips {
    "127.0.0.1";
    "172.0.0.0"/8;
}

backend default {
    .host = "aspnet-app";
    .port = "8080";
    .connect_timeout = 5s;
    .first_byte_timeout = 5s;
    .between_bytes_timeout = 10s;
}

sub vcl_recv {
    if (req.method == "PURGE") {
        if (!client.ip ~ local_purge_ips) {
             return(synth(405, "Not allowed."));
        }
        return (purge);
    }

    if (req.method == "GET" || req.method == "HEAD") {
        return (hash);
    }
    if (req.method != "GET" && req.method != "HEAD") {
        return (synth(405, "Método no permitido"));
    }
    if (req.http.X-Custom-Poison) {
        set req.http.X-Custom-Poison = regsub(req.http.X-Custom-Poison, "[\x00-\x1F]", "");
        # O forzar el envío sin sanitización (no siempre funciona)
        return (pass);  # Pasar directamente al backend sin cachear
    }

    #return (pipe);
}

sub vcl_backend_response {
    
    if (beresp.status >= 400 && beresp.status < 600) {
        
        set beresp.ttl = 60s; 
        return (deliver);
    }

    # Si el backend indica que el contenido es cacheable (Cache-Control: public, max-age)
    if (beresp.ttl > 0s && beresp.http.Cache-Control ~ "public" && beresp.http.Cache-Control ~ "max-age") {
        return (deliver);
    }

    return (deliver);
}

sub vcl_deliver {
    if (obj.hits > 0) {
        set resp.http.X-Cache = "HIT";
    } else {
        set resp.http.X-Cache = "MISS";
    }
}