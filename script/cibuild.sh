#!/usr/bin/env bash
set -e # halt script on error

bundle exec jekyll build ./_site
bundle exec htmlproofer ./_site
