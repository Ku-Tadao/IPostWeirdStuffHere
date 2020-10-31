import discord
from discord.ext import commands
import requests
import bs4 
from bs4 import BeautifulSoup as bs
import urllib

class League_of_Legends(commands.Cog, name="League of Legends"):
    """Information regarding League of Legends summoners"""


    def __init__(self, client):
        self.client = client
   
    
    @commands.command()
    async def google(self, ctx, *, givenquery):
        headers = {
            "x-rapidapi-key": "", #Enter RapidAPI key
            "x-rapidapi-host" :"google-search3.p.rapidapi.com"
        }
        query = {
            "q": givenquery,
            "num": 10
        }
        resp = requests.get("https://rapidapi.p.rapidapi.com/api/v1/search/" + urllib.parse.urlencode(query), headers=headers)
        results = resp.json()
        resultaten = results["results"]
        embed=discord.Embed(title="Search results for:", description=givenquery)
        embed.set_thumbnail(url="https://upload.wikimedia.org/wikipedia/commons/thumb/5/53/Google_%22G%22_Logo.svg/1200px-Google_%22G%22_Logo.svg.png")
        for resultaat in resultaten:
            embed.add_field(name=resultaat["title"], value=resultaat["link"], inline=False)
        await ctx.channel.send(embed=embed)

    @commands.command()
    async def youtube(self, ctx, *, givenquery):

        url = "https://youtube-search1.p.rapidapi.com/the%2520beatles"

        headers = {
            'x-rapidapi-host': "youtube-search1.p.rapidapi.com",
            'x-rapidapi-key': "" #Enter RapidAPI key
            }

        response = requests.request("GET", url, headers=headers)

        print(response.text)


         

    
def setup(client):
    client.add_cog(League_of_Legends(client))