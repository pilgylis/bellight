# Bellight.MediatR
Scan running assemblies and referenced libraries and register MediatR types to Microsoft.Extensions.DependencyInjection.

Refer to [MediatR Wiki](https://github.com/jbogard/MediatR/wiki) for usage.

## Register to Bellight.Core

```c#
services.AddBellightCore(options => {

    // other options
    // ...

    options.AddMediatR();
});

```