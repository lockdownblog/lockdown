build-osx: clean
	dotnet publish ./Lockdown/Lockdown.csproj  -r osx-x64 -p:PublishSingleFile=true --self-contained false -o ./publish/osx-x64
	
clean:
	rm -rf ./publish
