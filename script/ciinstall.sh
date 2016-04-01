#!/usr/bin/env bash
set -e # halt script on error

cd ..
git clone --depth=3 --branch=gh-pages https://github.com/BricksandMortar/plugin-doc-template.git
rm -rf plugin-doc-template/.git
cp -r -n plugin-doc-template/* $TRAVIS_BUILD_DIR/
cd $TRAVIS_BUILD_DIR
#gem install jekyll html-proofer rake rake-jekyll github-pages wdm
bundle install
