# generate a private key
openssl genrsa -out service.key 4096
openssl rsa -text -in service.key

# create a CSR using the multi-domain config
openssl req -new -subj "/C=EU/ST=Bulgaria/L=Sofia/O=HandGame/OU=HandGame Dev/CN=handgame.service" -key service.key -config service.conf -out service.csr
openssl req -text -in service.csr

# create a certificate using the CSR and sign it with the private key
openssl x509 -req -days 3650 -sha512 -in service.csr -extensions v3_req -extfile service.conf -signkey service.key -out service.crt
openssl x509 -text -in service.crt

# create a pfx
cat service.crt service.key > service.all
openssl pkcs12 -export -in service.all -out service.pfx -password pass:handgame
rm service.all
