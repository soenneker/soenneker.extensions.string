"""Recreate pinned comparison sources and opt-in local dependency wiring.

Use existing sibling repositories or the categorized C:/git/Soenneker layout.
The script never edits production sources or changes repository checkouts.
"""
from pathlib import Path
import json
import subprocess
import xml.etree.ElementTree as ET

audit = Path(__file__).resolve().parent
root = audit.parent.parent
repositories = json.loads((audit / 'baseline.json').read_text())
locations = {}
projects = {}
for name, info in repositories.items():
    category = name.split('.')[1]
    candidates = [audit.parent] if name == 'Soenneker.Extensions.String' else [
        root / info['repository'],
        root.parent / category / info['repository'],
        root / category / info['repository'],
    ]
    relative_project = Path(info['project']).relative_to(info['repository'])
    repository = next((path for path in candidates if (path / relative_project).is_file()), None)
    if repository is None:
        raise FileNotFoundError(f'Existing checkout for {name} not found. Checked: {candidates}')
    locations[name] = repository.resolve()
    projects[name] = (repository / relative_project).resolve()
    print(f'{name}: {repository}')

selected = [name for name in repositories if name not in
            ('Soenneker.Gen.EnumValues', 'Soenneker.Enums.ContentKinds')]

for name in selected:
    info = repositories[name]
    repository = locations[name]
    commit = info['commit']
    files = subprocess.check_output(
        ['git', '-C', str(repository), 'ls-tree', '-r', '--name-only', commit, 'src'], text=True).splitlines()
    for file in files:
        if not file.endswith('.cs'):
            continue
        source = subprocess.check_output(
            ['git', '-C', str(repository), 'show', f'{commit}:{file}'], text=True, encoding='utf-8')
        for namespace in sorted(selected, key=len, reverse=True):
            source = source.replace(namespace, 'Baseline.' + namespace)
        target = audit / 'Baseline' / name / file
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_text(source, encoding='utf-8')

dependency_root = ET.Element('Project')
for name, info in repositories.items():
    group = ET.SubElement(dependency_root, 'ItemGroup', Condition=f"'$(AuditLocalDependencies)' == 'true' and '$(MSBuildProjectName)' == '{name}'")
    for reference in ET.parse(projects[name]).iter('PackageReference'):
        dependency = reference.get('Include')
        if dependency not in repositories or dependency == 'Soenneker.Gen.EnumValues':
            continue
        ET.SubElement(group, 'PackageReference', Remove=dependency)
        ET.SubElement(group, 'ProjectReference', Include=str(projects[dependency]))

project_root = ET.Element('Project')
group = ET.SubElement(project_root, 'ItemGroup')
for name in selected:
    ET.SubElement(group, 'ProjectReference', Include=str(projects[name]))

# Explicit opt-in is required, and unrelated build configuration is never overwritten.
target = audit / 'Audit.Dependencies.targets'
for path, document in [(target, dependency_root), (audit / 'Audit.Projects.props', project_root)]:
    ET.indent(document, space='  ')
    ET.ElementTree(document).write(path, encoding='utf-8', xml_declaration=True)
print(f'Prepared pinned baseline sources and {target}')
print('Build with -p:DirectoryBuildTargetsPath=' + str(target))
print('and -p:AuditLocalDependencies=true. Generated wiring stays inside performance/.')
