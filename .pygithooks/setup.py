#!/usr/bin/env python
# -*- coding: utf-8 -*-


from setuptools import setup, find_packages


setup(
    name='pygithooks-unitybasegame',
    version='0.0.1',
    description='pygithooks for Unity BaseGame',
    packages=find_packages(),
    entry_points={
        'pygithook': [
            'hooks=pygithooks_unitybasegame:load_hooks',
            'filters=pygithooks_unitybasegame:load_filters'
        ]
    },
    install_requires=[
        'pygithook',
    ]
)