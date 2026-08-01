/**
 * Os dois servicos sobem separados, em portas proprias.
 * Rode-os com o profile "http" (dotnet run --launch-profile http) para evitar
 * o certificado autoassinado do https no navegador.
 */
export const API = {
  auth: 'http://localhost:7001',
  urls: 'http://localhost:7000',
} as const;
