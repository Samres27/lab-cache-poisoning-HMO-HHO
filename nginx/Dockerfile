# nginx/Dockerfile
FROM nginx:alpine 

RUN rm /etc/nginx/conf.d/default.conf

COPY nginx.conf /etc/nginx/nginx.conf

# Opcional: Crear un directorio para los logs si lo necesitas fuera de la configuración predeterminada
# RUN mkdir -p /var/log/nginx
# RUN touch /var/log/nginx/access.log /var/log/nginx/error.log