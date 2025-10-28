# ./db/Dockerfile
FROM mysql:8.0
ENV MYSQL_ROOT_PASSWORD=root
ENV MYSQL_DATABASE=CloneEbayDB
COPY clone_ebay_mysql_schema.sql /docker-entrypoint-initdb.d/
EXPOSE 3306