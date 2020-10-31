import discord
from discord.ext import commands

class Calc(commands.Cog):

    def __init__(self, client):
        self.client = client
   

    @commands.command()
    async def add(self,ctx,a:int,b:int): await ctx.send(a+b)
    @commands.command()
    async def sub(self,ctx,a:int,b:int): await ctx.send(a-b)
    @commands.command()
    async def multiply(self,ctx,a:int,b:int): await ctx.send(a*b)
    @commands.command() 
    async def divide(self,ctx,a:int,b:int): await ctx.send(a/b)

def setup(client):
    client.add_cog(Calc(client))
