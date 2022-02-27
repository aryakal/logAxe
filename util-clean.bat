FOR /F "tokens=*" %%G IN ('DIR /B /AD /S bin') DO rm -rvf "%%G"
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO rm -rvf "%%G"