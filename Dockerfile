FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

RUN unzip Payments.Backend.zip
RUN rm Payments.Backend.zip

ENTRYPOINT [ "Payments.Backend/Payments.Backend" ]
