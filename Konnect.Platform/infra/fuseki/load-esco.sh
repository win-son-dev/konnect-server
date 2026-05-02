#!/usr/bin/env bash
#
# Loads the ESCO (European Skills, Competences, Qualifications and
# Occupations) RDF ontology into the Fuseki `esco` dataset using tdb2.tdbloader.
#
# Run this AFTER `docker compose up -d` has Fuseki healthy. Run from the host:
#
#   docker compose exec fuseki bash /fuseki-init/load-esco.sh
#
# Idempotent: if the dataset already has triples, it skips the download.
# ESCO is published by the European Commission under CC-BY 4.0 — attribution
# is recorded in the root README.md.

set -euo pipefail

ESCO_VERSION="1.2.0"
ESCO_BUNDLE_URL="https://esco.ec.europa.eu/sites/default/files/2024-02/ESCO_dataset_release_${ESCO_VERSION}.zip"
ESCO_STAGING_DIR="/fuseki/databases/esco-staging"
ESCO_DATASET_DIR="/fuseki/databases/esco"
ESCO_DOWNLOAD_DIR="/tmp/esco-download"

triple_count_query='ASK { ?s ?p ?o }'
existing_triples=$(curl -fsS \
  "http://localhost:3030/esco/sparql?query=$(printf %s "$triple_count_query" | jq -sRr @uri)" \
  -H "Accept: application/sparql-results+json" 2>/dev/null \
  | jq -r '.boolean // false')

if [[ "$existing_triples" == "true" ]]; then
  echo "ESCO dataset already populated — skipping load. Drop the dataset to re-import."
  exit 0
fi

echo "Downloading ESCO ${ESCO_VERSION} from EC publication site..."
mkdir -p "$ESCO_DOWNLOAD_DIR"
curl -fsSL --retry 3 -o "$ESCO_DOWNLOAD_DIR/esco.zip" "$ESCO_BUNDLE_URL"

echo "Extracting RDF/Turtle files..."
unzip -q -o "$ESCO_DOWNLOAD_DIR/esco.zip" -d "$ESCO_DOWNLOAD_DIR/extracted"

mkdir -p "$ESCO_STAGING_DIR"
echo "Loading triples into staging TDB2 store at $ESCO_STAGING_DIR..."
tdb2.tdbloader \
  --loc="$ESCO_STAGING_DIR" \
  "$ESCO_DOWNLOAD_DIR/extracted"/*.ttl

echo "Atomic-swap staging into live dataset at $ESCO_DATASET_DIR..."
rm -rf "$ESCO_DATASET_DIR.previous"
if [[ -d "$ESCO_DATASET_DIR" ]]; then
  mv "$ESCO_DATASET_DIR" "$ESCO_DATASET_DIR.previous"
fi
mv "$ESCO_STAGING_DIR" "$ESCO_DATASET_DIR"

echo "Cleaning up download artifacts..."
rm -rf "$ESCO_DOWNLOAD_DIR"

echo "Done. Restart the fuseki container to pick up the swapped dataset:"
echo "  docker compose restart fuseki"
