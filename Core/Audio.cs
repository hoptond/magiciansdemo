using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using OpenAL;

namespace Magicians
{
	class Audio
	{
		readonly Game game;
		readonly SortedList<string, SoundEffect> sounds = new SortedList<string, SoundEffect>();
		public string currentMusic = "";
		public string currentAmbience = "";
		public SoundEffectInstance ambience { get; private set; }
		SortedList<string, SoundEffectInstance> effectInstances = new SortedList<string, SoundEffectInstance>();
		public OggStream music;
		SortedList<string, OggStream> musicFiles = new SortedList<string, OggStream>();
		public void SetMusic(string se)
		{
			SetMusic(se, true);
		}
		public void SetMusic(string se, bool looping)
		{
			float volume = game.settings.musVolume;
			if (game.settings.mutedMusic)
				volume = 0;
			if (File.Exists(game.Content.RootDirectory + game.PathSeperator + "Music" + game.PathSeperator + se + ".ogg"))
			{
				if (currentMusic == se)
					return;
				currentMusic = se;
				if (music != null)
					music.Stop();
			}
			else
			{
				currentMusic = "";
				music.Stop();
				Console.WriteLine("PANIC: MUSIC FILE " + se + " NOT FOUND");
				return;
			}
			if (se != currentMusic || music == null)
			{
				if (music != null)
				{
					music.Stop();
					music.Dispose();
					music = null;
				}
				music = new OggStream(game.Content.RootDirectory + game.PathSeperator + "Music" + game.PathSeperator + se + ".ogg");
				music.IsLooped = looping;
				music.Volume = volume;
				music.Play();
				currentMusic = se;
			}
			else
			{
				if (music.IsFinished())
				{
					music.Stop();
					music.Dispose();
					music = null;
					music = new OggStream(game.Content.RootDirectory + game.PathSeperator + "Music" + game.PathSeperator + se + ".ogg");
					music.IsLooped = looping;
					music.Volume = volume;
					music.Play();
					currentMusic = se;
				}
			}
		}
		public void SetAmbience(string se, int vol)
		{
			try
			{
				float volume = game.settings.soundVolume;
				if (game.settings.mutedSound)
					volume = 0;
				if (se != currentAmbience)
				{
					if (ambience != null)
					{
						ambience.Stop();
						ambience.Dispose();
					}
					var soundEffect = game.Content.Load<SoundEffect>("Ambience\\" + se);
					ambience = soundEffect.CreateInstance();
					ambience.IsLooped = true;
					ambience.Volume = (volume / 100) * vol;
					ambience.Play();
					currentAmbience = se;
				}
				else
				{
					if (ambience.State != SoundState.Playing)
					{
						try
						{
							var sound = game.Content.Load<SoundEffect>("Ambience\\" + se);
							ambience = sound.CreateInstance();
							ambience.IsLooped = true;
							ambience.Volume = (volume / 100) * vol;
							ambience.Play();
							currentAmbience = se;
						}
						catch
						{
							Console.WriteLine("AUDIO: COULD NOT FIND AMBIENCE " + se + ", PLAYING SILENCE INSTEAD");
						}
					}
				}
			}
			catch
			{
				Console.WriteLine("AUDIO: COULD NOT FIND AMBIENCE " + se + ", PLAYING SILENCE INSTEAD");
			}
		}
		public void Update()
		{
			for (int i = 0; i < effectInstances.Keys.Count; i++)
			{
				if (effectInstances[effectInstances.Keys[i]].State == SoundState.Stopped)
				{
					effectInstances.Remove(effectInstances.Keys[i]);
					i--;
				}
			}
		}
		public void StopSound(string s)
		{
			if (effectInstances.ContainsKey(s))
			{
				effectInstances[s].Stop();
			}
		}
		public void PlaySound(string s, bool overrideSound)
		{
			if (!game.settings.mutedSound)
			{
				if (sounds.ContainsKey(s))
				{
					if (effectInstances.ContainsKey(s))
					{
						if (!overrideSound)
						{
							if (effectInstances[s].State == SoundState.Playing)
							{
								return;
							}
						}
						effectInstances[s].Stop();
						effectInstances.Remove(s);
						effectInstances.Add(s, sounds[s].CreateInstance());
						effectInstances[s].Volume = game.settings.soundVolume;
						effectInstances[s].Play();
					}
					else
					{
						effectInstances.Add(s, sounds[s].CreateInstance());
						effectInstances[s].Volume = game.settings.soundVolume;
						effectInstances[s].Play();
					}
				}
				else
				{
					Console.WriteLine("PANIC: ATTEMPTED TO PLAY SOUND: " + s + ", BUT SOUND NOT FOUND IN CONTENT FOLDER");
				}
			}
		}
		public void PlaySound(string s, bool overrideSound, int vol)
		{
			if (!game.settings.mutedSound)
			{
				if (sounds.ContainsKey(s))
				{
					if (effectInstances.ContainsKey(s))
					{
						if (!overrideSound)
						{
							if (effectInstances[s].State == SoundState.Playing)
							{
								return;
							}
						}
						effectInstances[s].Stop();
						effectInstances.Remove(s);
						effectInstances.Add(s, sounds[s].CreateInstance());
						effectInstances[s].Volume = (game.settings.soundVolume / 100) * vol;
						effectInstances[s].Play();
					}
					else
					{
						effectInstances.Add(s, sounds[s].CreateInstance());
						effectInstances[s].Volume = (game.settings.soundVolume / 100) * vol;
						effectInstances[s].Play();
					}
				}
				else
				{
					Console.WriteLine("PANIC: ATTEMPTED TO PLAY SOUND: " + s + ", BUT SOUND NOT FOUND IN CONTENT FOLDER");
				}
			}
		}
		public void PlaySound(string s, bool overrideSound, float pan)
		{
			if (!game.settings.mutedSound)
			{
				if (sounds.ContainsKey(s))
				{
					if (effectInstances.ContainsKey(s))
					{
						if (!overrideSound)
						{
							if (effectInstances[s].State == SoundState.Playing)
							{
								return;
							}
						}
						effectInstances[s].Stop();
						effectInstances.Remove(s);
						effectInstances.Add(s, sounds[s].CreateInstance());
						effectInstances[s].Pan = pan;
						effectInstances[s].Volume = game.settings.soundVolume;
						effectInstances[s].Play();
					}
					else
					{
						effectInstances.Add(s, sounds[s].CreateInstance());
						effectInstances[s].Volume = game.settings.soundVolume;
						effectInstances[s].Pan = pan;
						effectInstances[s].Play();
					}
				}
				else
				{
					Console.WriteLine("PANIC: ATTEMPTED TO PLAY SOUND: " + s + ", BUT SOUND NOT FOUND IN CONTENT FOLDER");
				}
			}
		}
		public Audio(Game game)
		{
			this.game = game;
			var files = Directory.GetFiles(game.Content.RootDirectory + game.PathSeperator + "Sound");
			for (
				int i = 0; i < files.Length; i++)
			{
				if (files[i].EndsWith(".xnb"))
				{
					var file = files[i].Substring(14, files[i].Length - 14);
					file = file.Substring(0, file.Length - 4);
					sounds.Add(file, game.Content.Load<SoundEffect>("Sound\\" + file));
				}
			}
		}
	}
}
