const aspireHttps = process.env['services__apiservice__https__0'];
const aspireHttp = process.env['services__apiservice__http__0'];

const target = aspireHttps ?? aspireHttp ?? 'http://localhost:52862';

if (!aspireHttps && !aspireHttp) {
  throw new Error('services__apiservice__https__0/services__apiservice__http__0 are missing');
}

module.exports = {
  "/api": {
    target,
    changeOrigin: true,
    secure: process.env["NODE_ENV"] !== "development",
    logLevel: "debug" 
  },
};
