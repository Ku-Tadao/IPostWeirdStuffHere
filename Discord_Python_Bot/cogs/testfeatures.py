import discord
from discord.ext import commands
import requests
import bs4 
from bs4 import BeautifulSoup as bs

class testfeatures(commands.Cog, name="Test Features"):
    """me testing shit"""


    def __init__(self, client):
        self.client = client
   
    
    @commands.command()
    async def message(self, ctx):
            await ctx.channel.send(
"""```markdown
|test|test|test|test||test|test|
|---|---|---|---|---|
|test|test|test|test|test|test|
|test|test|test|test|test|test|
|test|test|test|test|test|test|
```""")
        

def setup(client):
    client.add_cog(testfeatures(client))