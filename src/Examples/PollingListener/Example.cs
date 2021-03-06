﻿using System;
using System.Net;
using System.Windows.Forms;
using Rug.Osc;

namespace PollingListener
{
	public partial class Example : Form
	{
		OscReceiver m_Receiver; 
		
		public Example()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Append a line to the output box
		/// </summary>
		/// <param name="line">the line to append</param>
		void AppendLine(string line)
		{
			m_Output.AppendText(line + Environment.NewLine);
			m_Output.Select(m_Output.TextLength, 0);
			m_Output.ScrollToCaret();
		}

		private void Connect_Click(object sender, EventArgs e)
		{
			// if there is already an instace dispose of it
			if (m_Receiver != null)
			{
				// disable the timer
				m_MessageCheckTimer.Enabled = false;

				// dispose of the reciever
				AppendLine("Disconnecting");
				m_Receiver.Dispose();
				m_Receiver = null;
			}

			// get the ip address from the address box 
			string addressString = m_AddressBox.Text;

			IPAddress ipAddress;

			// parse the ip address
			if (addressString.Trim().Equals("Any", StringComparison.InvariantCultureIgnoreCase) == true)
			{
				ipAddress = IPAddress.Any; 
			}
			else if (IPAddress.TryParse(addressString, out ipAddress) == false)
			{
				AppendLine(String.Format("Invalid IP address, {0}", addressString));

				return;
			}

			// create the reciever instance
			m_Receiver = new OscReceiver(ipAddress, (int)m_PortBox.Value);

			// tell the user
			AppendLine(String.Format("Listening on: {0}:{1}", ipAddress, (int)m_PortBox.Value));

			try
			{
				// connect to the socket 
				m_Receiver.Connect();
			}	
			catch (Exception ex)
			{
				AppendLine("Exception while connecting");
				AppendLine(ex.Message);

				m_Receiver.Dispose();
				m_Receiver = null;

				return;
			}

			// enable the timer
			m_MessageCheckTimer.Enabled = true; 
		}

		private void Listen_Tick(object sender, EventArgs e)
		{
			// if we are in a state to recieve
			if (m_Receiver != null &&
				m_Receiver.State == OscSocketState.Connected)
			{
				OscPacket packet;

				// try and get the next message
				while (m_Receiver.TryReceive(out packet) == true)
				{
					if (packet.Error == OscPacketError.None)
					{
						// write the message to the output
						AppendLine(packet.ToString());
					}
					else
					{
						AppendLine("Error reading packet, " + packet.Error);
						AppendLine(packet.ErrorMessage);
					}					
				}
			}
		}

		private void Clear_Click(object sender, EventArgs e)
		{
			// clear the output box
			m_Output.Clear(); 
		}

		private void Example_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (m_Receiver != null)
			{
				// dispose of the reciever
				m_Receiver.Dispose();
				m_Receiver = null; 
			}
		}
	}
}
