#!/usr/bin/env bash
set -e # halt script on error

cd ..
rm -rf plugin-doc-template
git clone --depth=3 --branch=gh-pages https://github.com/BricksandMortar/plugin-doc-template.git
cd plugin-doc-template
git checkout gh-pages
cd ..
rm -rf plugin-doc-template/.git
cp -r -n plugin-doc-template/* $TRAVIS_BUILD_DIR/
cd $TRAVIS_BUILD_DIR
#gem install jekyll html-proofer rake rake-jekyll github-pages wdm
bundle install
