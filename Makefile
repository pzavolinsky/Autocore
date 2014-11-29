.PHONY: all test coverage show package push-package
all: coverage
test: test-results/coverage.cov
coverage: test-results/coverage/project.html
package: pkg/Autocore.1.0.0.0.nupkg

MONO:=../mono-custom
NUGET:=../NuGet.exe

ASSEMBLIES:=Autocore/bin/Debug/Autocore.dll Autocore.Test/bin/Debug/Autocore.Test.dll
RELEASE:=Autocore/bin/Release/Autocore.dll

$(ASSEMBLIES):
	mdtool build

$(RELEASE):
	cd Autocore; mdtool build -c:Release

test-results/coverage.cov: $(ASSEMBLIES)
	@echo "=== Running tests ============="
	mkdir -p test-results/coverage
	. $(MONO)/mono-env
	LD_LIBRARY_PATH=$(MONO)/lib mono --debug --profile=monocov:outfile=test-results/coverage.cov,+[Autocore] $(MONO)/lib/mono/4.5/nunit-console.exe Autocore.Test/bin/Debug/Autocore.Test.dll
	mv TestResult.xml test-results

test-results/coverage/project.html: test-results/coverage.cov
	@echo "=== Computing code coverage ==="
	MONO_PATH=Autocore.Test/bin/Debug $(MONO)/bin/monocov --export-html=test-results/coverage test-results/coverage.cov
	xdg-open $@

show:
	xdg-open test-results/coverage/project.html

pkg/Autocore.1.0.0.0.nupkg: $(RELEASE)
	mkdir -p pkg
	cd pkg; ../$(MONO)/bin/mono ../$(NUGET) pack ../Autocore/Autocore.nuspec -Verbosity detailed

push-package: pkg/Autocore.1.0.0.0.nupkg
	XDG_CONFIG_HOME=~/.mono $(MONO)/bin/mono $(NUGET) push $< -Verbosity detailed