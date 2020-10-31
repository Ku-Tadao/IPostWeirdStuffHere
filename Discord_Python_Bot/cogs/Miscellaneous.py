import discord
from discord.ext import commands
import aiohttp


class Miscellaneous(commands.Cog):

    def __init__(self, client):
        self.client = client


    @commands.command()
    async def hello(self, ctx):
            if ctx.author.id == 315016232791441416 or ctx.author.id == 397435378258149377:
                await ctx.channel.send("Hello " + "boss " + str(ctx.author)[:-5] + "!")
            else:
                await ctx.channel.send("Hello, I am Salar and Kubi's test bot.")
    @commands.command()
    async def say(self, ctx, *args):
        boop = ""
        for arg in args:
            boop = boop + " " + arg
        await ctx.send("`"+boop+"`")  
    
    @commands.command()
    async def animal(self, ctx, arg):
        a = {"dog", "cat", "panda", "fox", "red_panda", "koala", "birb", "racoon", "kangaroo"}
        if arg.lower() in a:
            async with aiohttp.ClientSession() as cs:
                async with cs.get("https://some-random-api.ml/animal/" + arg.lower()) as r:
                
                    data = await r.json()
                    embed = discord.Embed(
                        title=data['fact'],
                        color = ctx.author.color
                    )
                    embed.set_image(url=data['image'])
                    await ctx.send(embed=embed)
        else:
            await ctx.send('*Error, use command as followed: `.anime "Dog", "Cat", "Panda", "Fox", "Red_panda", "Koala", "Birb", "Racoon" and "kangaroo"`*')
    @commands.command()
    async def meme(self, ctx):
        async with aiohttp.ClientSession() as cs:
            async with cs.get("https://some-random-api.ml/meme") as r:
                
                data = await r.json()
                embed = discord.Embed(
                    title= data['caption'],
                    color = ctx.author.color,
                    description= data['category']
                )
                embed.set_image(url=data['image'])
                await ctx.send(embed=embed)

    @commands.command()
    async def anime(self, ctx, arg):
        a = {"wink", "pat", "hug", "face-palm"}
        if arg.lower() in a:
            async with aiohttp.ClientSession() as cs:
                async with cs.get("https://some-random-api.ml/animu/" + arg.lower()) as r:
                    data = await r.json()
                    embed = discord.Embed(
                        title= arg.lower(),
                        color = ctx.author.color,
                    )
                    embed.set_image(url=data['link'])
                    await ctx.send(embed=embed)
        else:
            await ctx.send('*Error, use command as followed: `.anime "Wink", "Pat", "Hug" or "Face-palm"`*')


def setup(client):
    client.add_cog(Miscellaneous(client))