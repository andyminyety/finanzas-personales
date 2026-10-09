# 💰 Sistema de Gestión de Finanzas Personales

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-18-61DAFB?logo=react)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.0-3178C6?logo=typescript)](https://www.typescriptlang.org/)
[![Tailwind CSS](https://img.shields.io/badge/Tailwind_CSS-3.0-38B2AC?logo=tailwind-css)](https://tailwindcss.com/)
[![MongoDB](https://img.shields.io/badge/MongoDB_Atlas-47A248?logo=mongodb)](https://www.mongodb.com/atlas)

Aplicación web desarrollada como proyecto universitario para administrar y consultar las finanzas personales, facilitando el registro de ingresos, egresos y transacciones, así como el seguimiento detallado de la actividad financiera mensual.

---

## 🛠️ Stack Tecnológico

- **Backend:** ASP.NET Core (.NET 10)
- **Frontend:** React, TypeScript y Tailwind CSS
- **Base de Datos:** MongoDB Atlas
- **Autenticación:** JSON Web Tokens (JWT)

---

## ✨ Funcionalidades Principales

- 👤 **Gestión de Usuarios:** Control de perfiles, autenticación y autorizaciones con JWT.
- ⚙️ **Configuración Financiera:** Administración de tipos de ingresos, egresos, renglones y métodos de pago.
- 💵 **Registro de Movimientos:** Control completo de ingresos, egresos y transacciones.
- 🪪 **Validaciones Locales:** Validación integrada de Cédula de Identidad y RNC.
- 🔍 **Filtros Avanzados:** Búsqueda y filtrado dinámico de movimientos financieros.
- 📅 **Corte Mensual:** Módulo de corte de período financiero (automático y manual).
- 📊 **Dashboard Interactivo:** Resumen financiero visual mediante gráficos informativos.
- 📄 **Exportación de Datos:** Generación de reportes resumidos y detallados en formatos PDF y CSV.
- 🛡️ **Seguridad:** Control de acceso por roles y validaciones de datos estrictas.

---

## 📂 Estructura del Proyecto

```text
.
├── backend/    # API REST, lógica de negocio y acceso a datos (ASP.NET Core)
├── frontend/   # Interfaz de usuario y componentes visuales (React + TS + Tailwind)
```

---

## 📋 Requisitos Previos

Asegúrate de contar con las siguientes herramientas instaladas antes de ejecutar el proyecto:

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20.x o superior](https://nodejs.org/)
- Cuenta activa e instancia configurada en [MongoDB Atlas](https://www.mongodb.com/cloud/atlas)

---

## 🚀 Instalación y Configuración

### 1. Clonar el repositorio
```bash
git clone https://github.com/tu-usuario/tu-repositorio.git
cd tu-repositorio
```

### 2. Configurar el Backend
1. Navega al directorio del backend:
   ```bash
   cd backend
   ```
2. Configura tu cadena de conexión a MongoDB Atlas y la clave secreta de JWT en `appsettings.json` o mediante variables de entorno:
   ```json
   {
     "ConnectionStrings": {
       "MongoDb": "tu_string_de_conexion_mongodb"
     },
     "Jwt": {
       "Secret": "tu_clave_secreta_super_segura"
     }
   }
   ```
3. Ejecutar la API:
   ```bash
   dotnet run
   ```

### 3. Configurar el Frontend
1. Abre una nueva terminal y navega al directorio del frontend:
   ```bash
   cd frontend
   ```
2. Instala las dependencias:
   ```bash
   npm install
   ```
3. Inicia el servidor de desarrollo:
   ```bash
   npm run dev
   ```

---

## 📄 Licencia

Este proyecto fue desarrollado con fines académicos.