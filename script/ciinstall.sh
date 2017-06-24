#!/usr/bin/env bash
set -e # halt script on error

# Clone down the template, get rid of .git, and copy it on top of the contents of this pages-ci branch
cd ..
git clone --depth=3 --branch=gh-pages https://github.com/BricksandMortar/plugin-doc-template.git
rm -rf plugin-doc-template/.git
cp -r -n -p plugin-doc-template/* $TRAVIS_BUILD_DIR/
cd $TRAVIS_BUILD_DIR

# Install gems from the gemfile
bundle install

# Replace the suffix $VERSION_NUMBER with the env variable $TRAVIS_BUILD_NUMBER on all assets to prevent caching 
sed -i -e 's/$VERSION_NUMBER/'$TRAVIS_BUILD_NUMBER'/g' ./_layouts/*.html