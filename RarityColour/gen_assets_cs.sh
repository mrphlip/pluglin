#!/bin/bash
set -euo pipefail
TAB=$'\t'
(
	echo "namespace RarityColour;"
	echo "public class Assets {"
	for png in assets/*.png
	do
		echo "${TAB}public readonly static byte[] PNG$(basename "$png" .png) = new byte[] {"
		xxd -p "$png" | sed -E 's/(..)/0x\1,/g;s/^/\t\t/'
		echo "${TAB}};"
	done
	echo "}"
) > Assets.cs
