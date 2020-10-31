import discord
import os
from discord.ext import commands, tasks
from discord import File
import datetime

client = commands.Bot(command_prefix=".")

@client.event
async  def on_ready():
    await client.change_presence(activity=discord.Game('online'))
    fridayCheck.start()

@tasks.loop(hours=1)
async def fridayCheck():
    now = datetime.datetime.now()
    if datetime.datetime.today().weekday() == 4 and now.hour == 15:
        channel = client.get_channel(722540277050376266)
        await channel.send(file=discord.File('fridaySailer.mp4'))

for filename in os.listdir('./cogs'):
    if filename.endswith(".py"):
        client.load_extension(f'cogs.{filename[:-3]}')


client.run('')  #Enter Bot Token