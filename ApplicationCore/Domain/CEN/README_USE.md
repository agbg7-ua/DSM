Uso sugerido de los CEN:

- Instanciar via DI con la implementación concreta de repositorio y IUnitOfWork
- Llamar a los métodos Create/Modify/Destroy expuestos
- No deben orquestar múltiples repos: eso es trabajo de los CP
