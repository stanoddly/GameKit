#!/bin/sh

set -e

pushd GameKit.Collections

jinja2 MultiArray.cs.jinja > MultiArray.cs
jinja2 MultiMap.cs.jinja > MultiMap.cs
jinja2 FastList.cs.jinja > FastList.cs
jinja2 DenseSlotMap.cs.jinja > DenseSlotMap.cs
jinja2 IConstructible.cs.jinja > IConstructible.cs

popd
