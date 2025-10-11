#!/bin/bash

# Выполняем команду в MongoDB с использованием mongosh
mongosh \
  -u "${MONGO_INITDB_ROOT_USERNAME}" \
  -p "${MONGO_INITDB_ROOT_PASSWORD}" \
  --eval "try { 
    rs.status() 
  } catch (err) { 
    rs.initiate({
      _id: 'rs0',
      members: [
        { _id: 0, host: 'mongo:27017', priority: 1 }
      ]
    }) 
  }"