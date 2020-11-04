import discord
from discord.ext import commands
import requests
import bs4 
from bs4 import BeautifulSoup as bs

class Csgo(commands.Cog, name="CS:GO"):
    """Information regarding League of Legends summoners"""


    def __init__(self, client):
        self.client = client
   
    
    @commands.command()
    async def csgo(self, ctx, *, steamid):
        message = await ctx.channel.send("Loading")
        headers = {'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:82.0) Gecko/20100101 Firefox/82.0'}
        try:
            website = requests.get("https://tracker.gg/csgo/profile/steam/"+steamid+"/overview", headers=headers)
            websitestring = "https://tracker.gg/csgo/profile/steam/"+steamid+"/overview"
            parser = bs4.BeautifulSoup(website.text, 'lxml')
            soup=bs(website.content,"html.parser")            
            afb=soup.find("img",class_="ph-avatar__image")
            image = afb["src"]
            wrappers=soup.find_all("div",class_="wrapper")
        
            getnaam = parser.select('.trn-ign__username')
            naam = getnaam[0].getText().strip()
            embed=discord.Embed(title="CS:GO")
            embed.set_author(name=naam, url=websitestring)
            embed.set_thumbnail(url=image)
            for wrapper in wrappers:
                names = wrapper.find_all("span",class_="name")
                values = wrapper.find_all("span",class_="value")
                statname = ""
                statvalue = ""
                for name in names:
                    statname = name.text
                for value in values:
                    statvalue = value.text

                embed.add_field(name=statname,value=statvalue, inline=True)
            await message.edit(content="Grabbing completed, preparing message!")
            await message.delete()
            await ctx.channel.send(embed=embed)
        except:
                website = requests.get("https://tracker.gg/csgo/profile/steam/"+steamid+"/overview", headers=headers)
                websitestring = "https://tracker.gg/csgo/profile/steam/"+steamid+"/overview"
                parser = bs4.BeautifulSoup(website.text, 'lxml')
                soup=bs(website.content,"html.parser")            
                image="https://files.catbox.moe/9d1p6z.png"
                errors=soup.find_all("div",class_="error-message")
                embed=discord.Embed(title="CS:GO")
                embed.set_thumbnail(url=image)
                for error in errors:
                    error = error.text.strip()
                    embed.add_field(name="ERROR", value=error, inline=False)
                    embed.set_footer(text="https://support.steampowered.com/kb_article.php?ref=4113-YUDH-6401")

                await message.edit(content="Grabbing completed, preparing message!")
                await message.delete()
                await ctx.channel.send(embed=embed)
    
def setup(client):
    client.add_cog(Csgo(client))





