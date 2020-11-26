import discord
from discord.ext import commands
import requests
import bs4 
from bs4 import BeautifulSoup as bs

class NetworkCalc(commands.Cog, name="IP / Subnet Calculator"):
    """IP / Subnet mask calculator"""


    def __init__(self, client):
        self.client = client
   
    
    @commands.command()
    async def ncalc(self, ctx, adress, netmask):
        netmask = int(netmask)
        if netmask <= 32:
            message = await ctx.channel.send("Loading")

            headers = {'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:82.0) Gecko/20100101 Firefox/82.0'}
            website = requests.get("http://jodies.de/ipcalc?host="+adress+"&mask1="+str(netmask)+"&mask2=", headers=headers)
            soup=bs(website.content,"html.parser")            
            gettexts=soup.findAll("font")
            getdoublefonts=gettexts[13].findAll("font")


            print(gettexts[0].text)           
            await message.edit(content="Grabbing completed, preparing message!")
            embed=discord.Embed()
            embed.add_field(name=gettexts[0].text, value=gettexts[1].text, inline=True)
            embed.add_field(name="Binary:", value=gettexts[2].text, inline=True)
            embed.add_field(name="-----------------------------------------", value="-", inline=False)
            embed.add_field(name=gettexts[3].text, value=gettexts[4].text, inline=True)
            embed.add_field(name="Binary:", value=gettexts[5].text, inline=True)
            embed.add_field(name="-----------------------------------------", value="-", inline=False)
            embed.add_field(name=gettexts[9].text, value=gettexts[10].text, inline=True)
            embed.add_field(name="Binary:", value=gettexts[11].text+gettexts[12].text[:-1], inline=True)
            embed.add_field(name="-----------------------------------------", value="-", inline=False)
            embed.add_field(name=getdoublefonts[1].text, value=getdoublefonts[2].text, inline=True)
            embed.add_field(name="Binary:", value=getdoublefonts[3].text, inline=True)
            embed.add_field(name="-----------------------------------------", value="-", inline=False)
            embed.add_field(name=getdoublefonts[4].text, value=getdoublefonts[5].text, inline=True)
            embed.add_field(name="Binary:", value=getdoublefonts[6].text, inline=True)
            embed.add_field(name="-----------------------------------------", value="-", inline=False)
            embed.add_field(name=getdoublefonts[7].text, value=getdoublefonts[8].text, inline=True)
            embed.add_field(name="Binary:", value=getdoublefonts[9].text, inline=True)
            embed.add_field(name="-----------------------------------------", value="-", inline=False)
            embed.add_field(name=getdoublefonts[10].text, value=getdoublefonts[11].text, inline=True)
            await ctx.send(embed=embed)
        else:
            await ctx.channel.send("*Error, use command as followed: `.ncalc ipadress subnet`*")        

def setup(client):
    client.add_cog(NetworkCalc(client))