// Gera src/types/api.generated.ts a partir do OpenAPI da API (npm run gen:types).
const s=require(require('path').resolve(process.argv[2]));
const S=s.components.schemas;
let CUR="";const KEEP_NULL_ARR=new Set(["RotaDto.geometria"]);
const isEnum=n=>!!(S[n]&&S[n].enum);
const isReq=n=>/(Request|Input|SalvarDto|SalvarRequest)$/.test(n)||['CondicaoRegra','OpcaoSalvarDto','IntegranteSalvarDto','PontoSalvarDto','FaixaSalvarDto','CriterioSalvarDto'].includes(n);
function t(r,resp){
  if(r.$ref){const n=r.$ref.split('/').pop(); return (resp&&!isEnum(n))? n+' | null' : n;}
  let base;
  if(r.type==='array'){const it=t(r.items,false); base=(it.includes('|')?'('+it+')':it)+'[]'; if(resp && !KEEP_NULL_ARR.has(CUR)) return base;}
  else if(r.type==='object'&&r.additionalProperties) base='Record<string, '+t(r.additionalProperties,false)+'>';
  else if(r.type==='integer'||r.type==='number') base='number';
  else if(r.type==='string') base='string';
  else if(r.type==='boolean') base='boolean';
  else base='unknown';
  return r.nullable? base+' | null': base;
}
let out=`/* eslint-disable */
// Tipos gerados a partir de farol/docs/openapi.json (OpenAPI ${s.openapi}).
// Enums são serializados como strings (nomes PascalCase do C#).
// Propriedades de DTOs de resposta são obrigatórias (o servidor sempre as envia);
// referências a objetos são anuláveis. DTOs de requisição têm propriedades opcionais.

`;
for(const [n,sc] of Object.entries(S)){
  if(sc.enum){out+=`export type ${n} = ${sc.enum.map(v=>JSON.stringify(v)).join(' | ')};\nexport const ${n}Valores = [${sc.enum.map(v=>JSON.stringify(v)).join(', ')}] as const satisfies readonly ${n}[];\n\n`;continue;}
  const req=isReq(n);
  out+=`export interface ${n} {\n`;
  for(const [k,v] of Object.entries(sc.properties||{})){CUR=n+"."+k;
    out+=`  ${k}${req?'?':''}: ${t(v,!req)};\n`;
  }
  out+=`}\n\n`;
}
process.stdout.write(out);
