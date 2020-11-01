import discord
from discord.ext import commands
import grequests
import urllib

class League_of_Legends(commands.Cog, name="League of Legends"):
    """Information regarding League of Legends summoners"""


    def __init__(self, client):
        self.client = client

    @commands.command()
    async def google(self, ctx, *, givenquery):
        headers = {
            "x-rapidapi-key": "0519994bedmsh6d54e5250fddec6p1bfba4jsn8d8846906f0e", 
            "x-rapidapi-host" :"google-search3.p.rapidapi.com"
        }
        query = {
            "q": givenquery,
            "num": 10
        }
        resp = grequests.get("https://rapidapi.p.rapidapi.com/api/v1/search/" + urllib.parse.urlencode(query), headers=headers)
        results = resp.json()
        resultaten = results["results"]
        embed=discord.Embed(title="Search results for:", description=givenquery)
        embed.set_thumbnail(url="https://upload.wikimedia.org/wikipedia/commons/thumb/5/53/Google_%22G%22_Logo.svg/1200px-Google_%22G%22_Logo.svg.png")
        for resultaat in resultaten:
            embed.add_field(name=resultaat["title"], value=resultaat["link"], inline=False)
        await ctx.channel.send(embed=embed)
        
def setup(client):
    client.add_cog(League_of_Legends(client))