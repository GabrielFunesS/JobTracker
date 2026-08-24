# 🚀 JobTracker - Gestor Estratégico de Postulaciones

Aplicación nativa multiplataforma desarrollada en **.NET 9** diseñada para centralizar, gestionar y medir el embudo de postulaciones laborales mediante metodologías ágiles.

## 📄 1. Resumen Ejecutivo
El objetivo de la fase inicial (MVP) es reemplazar el seguimiento manual en hojas de cálculo mediante un tablero Kanban interactivo y un sistema de alertas. La aplicación opera de manera 100% local (Offline), sentando una base arquitectónica limpia para futuras fases de sincronización en la nube y asistencia mediante Inteligencia Artificial.

## 🛠️ 2. Arquitectura y Stack Tecnológico
El proyecto adopta una **Arquitectura de Cortes Verticales (Vertical Slice Architecture)** implementando un patrón CQRS simplificado y preparación para el paradigma *Offline-First*.
*   **Front-end:** Blazor Hybrid con .NET MAUI (Interfaz nativa compartida para Escritorio y Móvil).
*   **Base de Datos Local:** SQLite (Almacenamiento persistente en el dispositivo).
*   **Escritura (Commands):** Entity Framework Core.
*   **Lectura (Queries):** Dapper (Micro-ORM para consultas de alto rendimiento en la UI).
*   **Inyección de Dependencias:** Uso estricto de interfaces (`IJobTrackerService`) para permitir la futura migración a API sin modificar los componentes visuales.

## 🗂️ 3. Estructura de Datos Base (Dominio)
Entidad central `JobApplication`:
*   **Identificación:** ID, Empresa, Puesto, URL del aviso.
*   **Clasificación:** Canal de Postulación, Estado Actual (Enviado, Entrevista, Rechazado, Oferta).
*   **Métricas:** Fecha de aplicación, Fecha de último seguimiento.
*   **Complementarios:** Salario esperado, Notas técnicas.

---

## 🎯 4. Historias de Usuario (Fase 1 - MVP Local)

### Gestión de Postulaciones
- [ ] **US 01:** Como usuario, quiero poder agregar una nueva postulación (Empresa, Puesto, URL, Fecha) para registrar mi actividad.
- [ ] **US 02:** Como usuario, quiero poder editar los detalles de una postulación (ej. agregar notas después de una entrevista) para mantener la información actualizada.
- [ ] **US 03:** Como usuario, quiero poder eliminar una postulación creada por error.

### Tablero Visual (Kanban)
- [ ] **US 04:** Como usuario, quiero ver todas mis postulaciones activas en un tablero dividido por columnas según su estado.
- [ ] **US 05:** Como usuario, quiero poder cambiar el estado de una postulación para reflejar mi avance en los procesos.

### Motor de Alertas (Reglas de Negocio)
- [ ] **US 06:** Como usuario, quiero que el sistema resalte las postulaciones en estado "Enviado" que llevan más de **5 días hábiles sin actualización**, para recordar enviar un mensaje de *Follow-Up*.
- [ ] **US 07:** Como usuario, quiero que el sistema sugiera archivar postulaciones con más de **30 días de inactividad** para mantener mi tablero limpio.

---

## 🚀 5. Roadmap Futuro (Backlog)

### Fase 2: Sincronización Multiplataforma (Cloud)
- [ ] Creación de Backend externo usando **.NET Minimal APIs**.
- [ ] Implementación de base de datos en la nube y login con **Google Auth (OAuth 2.0)**.
- [ ] Implementación de arquitectura **Offline-First (Patrón Outbox / Sync Queue)** para garantizar que la app móvil funcione sin internet y sincronice automáticamente al recuperar la conexión.

### Fase 3: Asistente de IA (Agente de Empleabilidad)
- [ ] Integración con APIs de Modelos de Lenguaje (OpenAI / Gemini).
- [ ] **Auto-Onboarding:** Lectura y estructuración automática de datos del CV en formato PDF.
- [ ] **Entrenador de Entrevistas:** Análisis de la URL de la vacante para sugerir áreas de estudio y autogenerar preguntas probables de entrevista junto con guiones de respuesta basados en el **Método STAR**.
