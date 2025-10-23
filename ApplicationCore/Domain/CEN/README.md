CEN (Componentes Entidad de Negocio) scaffold.

Aquí se deben implementar componentes sin estado por cada entidad que expongan operaciones CRUD ligeras y usen las interfaces de repositorio definidas en `ApplicationCore/Domain/Repositories`.

Convención: métodos síncronos y operan sobre `long` como clave primaria.
