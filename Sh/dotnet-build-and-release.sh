#!/bin/sh

#    Simple script that publishes and uploads a dotnet github release for three desktop platforms
#    Written by bajtixone (https://github.com/Bajtix) (https://bajtix.xyz/)

#    If you decide to use this script feel free to do so, I don't require credit, but if it's gonna be open source it'd be sick if you were to keep this comment.

#    How to use:
#    Put this script in the folder where your solution is, create a 'Build' folder and add it to your gitignore. Then just run the script, it's that easy 

echo "What is the release tag?"
read tag

git add .
git commit -S -m "release commit $tag"
git push

dotnet publish -c Release --os win --sc -o Build/win/
dotnet publish -c Release --os linux --sc -o Build/lin/
dotnet publish -c Release --os osx --sc -o Build/mac/

cd Build

zip -r windows.zip win/
zip -r linux.zip lin/
zip -r osx.zip mac/



gh release create $tag

gh release upload $tag windows.zip --clobber
gh release upload $tag linux.zip --clobber
gh release upload $tag osx.zip --clobber
